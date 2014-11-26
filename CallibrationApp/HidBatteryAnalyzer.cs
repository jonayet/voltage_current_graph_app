using System;
using INFRA.USB;
using INFRA.USB.HelperClasses;

namespace CallibrationApp
{
    public class HidBatteryAnalyzer
    {
        public event AnalogDataReceivedEventHandler OnAnalogDataReceived;
        public int ActualVoltageValue { get; private set; }
        public float VoltageConstant { get; private set; }
        public float VoltageOffset { get; private set; }
        public float Voltage { get; private set; }
        public int ActualCurrentValue { get; private set; }
        public float CurrentConstant { get; private set; }
        public float CurrentOffset { get; private set; }
        public float Current { get; private set; }

        private const int READ_INDEX_OF_VOLTAGE_DATA = 0;
        private const int SIZE_OF_VOLTAGE_DATA = 50;
        private const int READ_INDEX_OF_CURRENT_DATA = 50;
        private const int SIZE_OF_CURRENT_DATA = 2;
        private const int READ_INDEX_OF_VOLTAGE_CONSTANT = 52;
        private const int READ_INDEX_OF_VOLTAGE_OFFSET = 56;
        private const int READ_INDEX_OF_CURRENT_CONSTANT = 58;
        private const int READ_INDEX_OF_CURRENT_OFFSET = 62;

        private const int CMD_SET_CALIBRATION = 0xA3;
        private const int WRITE_INDEX_OF_CMD_SET_CALIBRATION = 0;
        private const int WRITE_INDEX_OF_VOLTAGE_CONSTANT = 1;
        private const int SIZE_OF_VOLTAGE_CONSTANT = 4;
        private const int WRITE_INDEX_OF_VOLTAGE_OFFSET = 5;
        private const int SIZE_OF_VOLTAGE_OFFSET = 2;
        private const int WRITE_INDEX_OF_CURRENT_CONSTANT = 7;
        private const int SIZE_OF_CURRENT_CONSTANT = 4;
        private const int WRITE_INDEX_OF_CURRENT_OFFSET = 11;
        private const int SIZE_OF_CURRENT_OFFSET = 2;

        private HidInterface _hidDevice;
        private readonly RingBuffer<int> _voltageBuffer;
        private readonly RingBuffer<int> _currentBuffer;
        private readonly int _sizeOfVoltageBuffer;
        private readonly int _sizeOfCurrentBuffer;
        private readonly int _requiredSizeOfVoltageBuffer;
        private readonly int _requiredSizeOfCurrentBuffer;

        public HidBatteryAnalyzer(HidInterface hidDevice, int sizeOfBuffer = 100)
        {
            _hidDevice = hidDevice;
            _hidDevice.OnReportReceived += _hidDevice_OnReportReceived;

            _sizeOfVoltageBuffer = sizeOfBuffer * 25;
            _sizeOfCurrentBuffer = sizeOfBuffer * 1;
            _requiredSizeOfVoltageBuffer = sizeOfBuffer * 25;
            _requiredSizeOfCurrentBuffer = sizeOfBuffer * 1;
            _voltageBuffer = new RingBuffer<int>(_sizeOfVoltageBuffer);
            _currentBuffer = new RingBuffer<int>(_sizeOfCurrentBuffer);
        }

        public void CalibrateVoltage(float actualVoltage1, int deviceVoltageData1, float actualVoltage2, int deviceVoltageData2)
        {
            // slope = (y1-y2)/(x1-x2)
            // offset = y-(slope*x)
            // y = offset + slope*x
            float vConstant = (actualVoltage1 - actualVoltage2)/(deviceVoltageData1 - deviceVoltageData2);
            float vOffset = actualVoltage1 - (deviceVoltageData1*vConstant);
            WriteVoltageCalibrationData(vConstant, 0);
        }

        public void CalibrateCurrent(float actualCurrent1, int deviceCurrentData1, float actualCurrent2, int deviceCurrentData2)
        {
            // slope = (y1-y2)/(x1-x2)
            // offset = y-(slope*x)
            // y = offset + slope*x
            float cConstant = (actualCurrent1 - actualCurrent2) / (deviceCurrentData1 - deviceCurrentData2);
            float cOffset = actualCurrent1 - (deviceCurrentData1 * cConstant);
            WriteCurrentCalibrationData(cConstant, 0);
        }

        public void WriteVoltageCalibrationData(float constant, float offset)
        {
            var calibrationData = new byte[HidOutputReport.UserDataLength];

            // sset command
            calibrationData[WRITE_INDEX_OF_CMD_SET_CALIBRATION] = CMD_SET_CALIBRATION;

            // set voltage constatnt
            Array.Copy(BitConverter.GetBytes(constant), 0, calibrationData, WRITE_INDEX_OF_VOLTAGE_CONSTANT, SIZE_OF_VOLTAGE_CONSTANT);

            // set voltage offset
            var vOffset = (short)(offset * 1000);
            Array.Copy(BitConverter.GetBytes(vOffset), 0, calibrationData, WRITE_INDEX_OF_VOLTAGE_OFFSET, SIZE_OF_VOLTAGE_OFFSET);

            // set current constatnt
            Array.Copy(BitConverter.GetBytes(0f), 0, calibrationData, WRITE_INDEX_OF_CURRENT_CONSTANT, SIZE_OF_CURRENT_CONSTANT);

            // set current offset
            Array.Copy(BitConverter.GetBytes((short)(0)), 0, calibrationData, WRITE_INDEX_OF_CURRENT_OFFSET, SIZE_OF_CURRENT_OFFSET);

            // write to the device
            _hidDevice.Write(calibrationData);
        }

        public void WriteCurrentCalibrationData(float constant, float offset)
        {
            var calibrationData = new byte[HidOutputReport.UserDataLength];

            // sset command
            calibrationData[WRITE_INDEX_OF_CMD_SET_CALIBRATION] = CMD_SET_CALIBRATION;

            // set voltage constatnt
            Array.Copy(BitConverter.GetBytes(0f), 0, calibrationData, WRITE_INDEX_OF_VOLTAGE_CONSTANT, SIZE_OF_VOLTAGE_CONSTANT);

            // set voltage offset
            Array.Copy(BitConverter.GetBytes((short)(0)), 0, calibrationData, WRITE_INDEX_OF_VOLTAGE_OFFSET, SIZE_OF_VOLTAGE_OFFSET);

            // set current constatnt
            Array.Copy(BitConverter.GetBytes(constant), 0, calibrationData, WRITE_INDEX_OF_CURRENT_CONSTANT, SIZE_OF_CURRENT_CONSTANT);

            // set current offset
            var cOffset = (short)(offset * 1000);
            Array.Copy(BitConverter.GetBytes(cOffset), 0, calibrationData, WRITE_INDEX_OF_CURRENT_OFFSET, SIZE_OF_CURRENT_OFFSET);

            // write to the device
            _hidDevice.Write(calibrationData);
        }

        private void _hidDevice_OnReportReceived(object sender, ReportRecievedEventArgs e)
        {
            // get 25 voltage reading samples
            var voltageData = GetIntArray(e.Report.UserData, READ_INDEX_OF_VOLTAGE_DATA, SIZE_OF_VOLTAGE_DATA);

            // get 1 current reading sample
            var currentData = GetIntArray(e.Report.UserData, READ_INDEX_OF_CURRENT_DATA, SIZE_OF_CURRENT_DATA);

            ExtractCalibrationData(e.Report.UserData);
            GetAverageVoltageAndCurrent(voltageData, currentData);

            // set data to RingBuffer
            _voltageBuffer.PutBlocking(voltageData, 0, voltageData.Length);
            _currentBuffer.PutBlocking(currentData, 0, currentData.Length);

            if (_voltageBuffer.LengthToRead >= _requiredSizeOfVoltageBuffer)
            {
                // read all avilable data
                var vData = new int[_sizeOfVoltageBuffer];
                var cData = new int[_sizeOfCurrentBuffer];
                _voltageBuffer.GetBlocking(vData, _requiredSizeOfVoltageBuffer);
                _currentBuffer.GetBlocking(cData, _requiredSizeOfCurrentBuffer);

                if (OnAnalogDataReceived != null)
                {
                    OnAnalogDataReceived(this, new AnalogDataReceivedEventArgs(null, null));
                }
            }
        }

        private void GetAverageVoltageAndCurrent(int[] voltageData, int[] currentData)
        {
            int sum = 0;
            foreach (int v in voltageData) { sum += v; }
            ActualVoltageValue = sum/voltageData.Length;
            Voltage = ActualVoltageValue * VoltageConstant + VoltageOffset;

            sum = 0;
            foreach (int c in currentData) { sum += c; }
            ActualCurrentValue = sum/currentData.Length;
            Current = ActualCurrentValue * CurrentConstant + CurrentOffset;
        }

        private void ExtractCalibrationData(byte[] rawData)
        {
            // get voltage constant & offfset
            VoltageConstant = BitConverter.ToSingle(rawData, READ_INDEX_OF_VOLTAGE_CONSTANT);
            VoltageOffset = BitConverter.ToInt16(rawData, READ_INDEX_OF_VOLTAGE_OFFSET) / 1000.0f;

            // get current constant & offfset
            CurrentConstant = BitConverter.ToSingle(rawData, READ_INDEX_OF_CURRENT_CONSTANT);
            CurrentOffset = BitConverter.ToInt16(rawData, READ_INDEX_OF_CURRENT_OFFSET) / 1000.0f;
        }

        private int[] GetIntArray(byte[] bytes, int startIndex, int length)
        {
            var result = new int[length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToInt16(bytes, startIndex + (i * 2));
            }
            return result;
        }
    }

    public delegate void AnalogDataReceivedEventHandler(object sender, AnalogDataReceivedEventArgs e);
    public class AnalogDataReceivedEventArgs : EventArgs
    {
        public float[] VoltageData, CurrentData;

        public AnalogDataReceivedEventArgs(float[] voltageData, float[] currentData)
        {
            VoltageData = voltageData;
            CurrentData = currentData;
        }
    }
}
