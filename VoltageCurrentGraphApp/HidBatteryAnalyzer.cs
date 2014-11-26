using System;
using System.Collections.Generic;
using System.Text;
using INFRA.USB;
using INFRA.USB.HelperClasses;

namespace VoltageCurrentGraphApp
{
    public class HidBatteryAnalyzer
    {
        public event AnalogDataReceivedEventHandler OnAnalogDataReceived;

        private readonly RingBuffer<int> _voltageBuffer;
        private readonly RingBuffer<int> _currentBuffer;
        private readonly int _sizeOfVoltageBuffer;
        private readonly int _sizeOfCurrentBuffer;
        private readonly int _requiredSizeOfVoltageBuffer;
        private readonly int _requiredSizeOfCurrentBuffer;

        public HidBatteryAnalyzer(HidInterface hidDevice, int sizeOfBuffer = 100)
        {
            hidDevice.OnReportReceived += _hidDevice_OnReportReceived;

            _sizeOfVoltageBuffer = sizeOfBuffer * 25;
            _sizeOfCurrentBuffer = sizeOfBuffer * 1;
            _requiredSizeOfVoltageBuffer = sizeOfBuffer * 25;
            _requiredSizeOfCurrentBuffer = sizeOfBuffer * 1;
            _voltageBuffer = new RingBuffer<int>(_sizeOfVoltageBuffer);
            _currentBuffer = new RingBuffer<int>(_sizeOfCurrentBuffer);
        }

        private void _hidDevice_OnReportReceived(object sender, ReportRecievedEventArgs e)
        {
            var voltageData = GetIntArray(e.Report.UserData, 0, 50);      // get 25 voltage reading samples
            var currentData = GetIntArray(e.Report.UserData, 50, 2);      // get 1 current reading sample
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
                    OnAnalogDataReceived(this, new AnalogDataReceivedEventArgs(vData, cData));
                }
            }
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
        public int[] VoltageData, CurrentData;

        public AnalogDataReceivedEventArgs(int[] voltageData, int[] currentData)
        {
            VoltageData = voltageData;
            CurrentData = currentData;
        }
    }
}
