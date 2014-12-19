using System;
using System.ComponentModel;
using System.Windows.Forms;
using INFRA.USB;

namespace CallibrationApp
{
    public partial class CalibrationUI : Form
    {
        private readonly HidDevice _hidDevice;
        private readonly HidBatteryAnalyzerManager _hidBatteryAnalyzerManager;
        private CalibrationStage _voltageCalibrationStage;
        private CalibrationStage _currentCalibrationStage;
        private float _actualVoltage1, _actualVoltage2;
        private float _actualCurrent1, _actualCurrent2;
        private int _deviceVoltageData1, _deviceVoltageData2;
        private int _deviceCurrentData1, _deviceCurrentData2;

        public CalibrationUI()
        {
            InitializeComponent();

            _hidDevice = new HidDevice(0x1FBD, 0x0004);
            _hidDevice.OnDeviceAttached += new EventHandler(hidPort_OnDeviceAttached);
            _hidDevice.OnDeviceRemoved += new EventHandler(hidPort_OnDeviceRemoved);
            _hidDevice.Connect();

            _hidBatteryAnalyzerManager = new HidBatteryAnalyzerManager(_hidDevice, 50);
            _hidBatteryAnalyzerManager.OnAnalogDataReceived += HidBatteryAnalyzerManagerOnAnalogDataReceived;
            _voltageCalibrationStage = CalibrationStage.Initial;
            _currentCalibrationStage = CalibrationStage.Initial;
        }

        void HidBatteryAnalyzerManagerOnAnalogDataReceived(object sender, AnalogDataReceivedEventArgs e)
        {
            // show voltage constant & offset
            ThreadHelperClass.SetText(this, voltageLabel, _hidBatteryAnalyzerManager.Voltage.ToString("0.0 V"));
            ThreadHelperClass.SetText(this, voltageConstantlabel, _hidBatteryAnalyzerManager.VoltageConstant.ToString("0.0000000"));
            ThreadHelperClass.SetText(this, voltageOffsetlabel, _hidBatteryAnalyzerManager.VoltageOffset.ToString("0.000"));

            // show current constant & offset
            ThreadHelperClass.SetText(this, currentLabel, _hidBatteryAnalyzerManager.Current.ToString("0.0 A"));
            ThreadHelperClass.SetText(this, currentConstantLabel, _hidBatteryAnalyzerManager.CurrentConstant.ToString("0.0000000"));
            ThreadHelperClass.SetText(this, currentOffsetLabel, _hidBatteryAnalyzerManager.CurrentOffset.ToString("0.000"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _hidDevice.Dispose();
            base.OnClosing(e);
        }

        private void hidPort_OnDeviceRemoved(object sender, EventArgs e)
        {
            ThreadHelperClass.SetText(this, deviceConnectedLabel, "Not Connected");
        }

        private void hidPort_OnDeviceAttached(object sender, EventArgs e)
        {
            ThreadHelperClass.SetText(this, deviceConnectedLabel, "Connected");
        }

        private void setVoltageButton_Click(object sender, EventArgs e)
        {
            switch (_voltageCalibrationStage)
            {
                case CalibrationStage.Initial:
                    var result = MessageBox.Show("Are you sure to Calibrate Voltage?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                    actualVoltageTextBox.Text = "";
                    setVoltageButton.Text = "Adjust Voltage1 (12 V)";
                    actualVoltageTextBox.Enabled = true;
                    _voltageCalibrationStage = CalibrationStage.Value1;
                    break;

                case CalibrationStage.Value1:
                    if (!float.TryParse(actualVoltageTextBox.Text, out _actualVoltage1))
                    {
                        MessageBox.Show("Enter value in correct format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _deviceVoltageData1 = _hidBatteryAnalyzerManager.ActualVoltageValue;
                    actualVoltageTextBox.Text = "";
                    setVoltageButton.Text = "Adjust Voltage2 (24 V)";
                    actualVoltageTextBox.Enabled = true;
                    _voltageCalibrationStage = CalibrationStage.Value2;
                    break;

                case CalibrationStage.Value2:
                    if (!float.TryParse(actualVoltageTextBox.Text, out _actualVoltage2))
                    {
                        MessageBox.Show("Enter value in correct format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _deviceVoltageData2 = _hidBatteryAnalyzerManager.ActualVoltageValue;
                    actualVoltageTextBox.Text = "";
                    setVoltageButton.Text = "Start Calibration";
                    actualVoltageTextBox.Enabled = false;
                    _voltageCalibrationStage = CalibrationStage.Initial;

                    _hidBatteryAnalyzerManager.CalibrateVoltage(_actualVoltage1, _deviceVoltageData1, _actualVoltage2, _deviceVoltageData2);
                    MessageBox.Show("Voltage calibration was successfull!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void setCurrentButton_Click(object sender, EventArgs e)
        {
            switch (_currentCalibrationStage)
            {
                case CalibrationStage.Initial:
                    var result = MessageBox.Show("Are you sure to Calibrate Current?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                    actualCurrentTextBox.Text = "";
                    //_hidBatteryAnalyzerManager.WriteVoltageCalibrationData(1, 0);
                    setCurrentButton.Text = "Adjust Current1 (5 A)";
                    actualCurrentTextBox.Enabled = true;
                    _currentCalibrationStage = CalibrationStage.Value1;
                    break;

                case CalibrationStage.Value1:
                    if (!float.TryParse(actualCurrentTextBox.Text, out _actualCurrent1))
                    {
                        MessageBox.Show("Enter value in correct format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _deviceCurrentData1 = _hidBatteryAnalyzerManager.ActualCurrentValue;
                    actualCurrentTextBox.Text = "";
                    setCurrentButton.Text = "Adjust Current2 (20 A)";
                    actualCurrentTextBox.Enabled = true;
                    _currentCalibrationStage = CalibrationStage.Value2;
                    break;

                case CalibrationStage.Value2:
                    if (!float.TryParse(actualCurrentTextBox.Text, out _actualCurrent2))
                    {
                        MessageBox.Show("Enter value in correct format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _deviceCurrentData2 = _hidBatteryAnalyzerManager.ActualCurrentValue;
                    actualCurrentTextBox.Text = "";
                    setCurrentButton.Text = "Start Calibration";
                    actualCurrentTextBox.Enabled = false;
                    _currentCalibrationStage = CalibrationStage.Initial;

                    _hidBatteryAnalyzerManager.CalibrateCurrent(_actualCurrent1, _deviceCurrentData1, _actualCurrent2, _deviceCurrentData2);
                    MessageBox.Show("Current calibration was successfull!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }

    enum CalibrationStage
    {
        Initial,
        Value1,
        Value2,
    }
}