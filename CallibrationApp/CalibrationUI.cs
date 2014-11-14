using System;
using System.ComponentModel;
using System.Windows.Forms;
using INFRA.USB;

namespace CallibrationApp
{
    public partial class CalibrationUI : Form
    {
        private HidInterface _hidDevice;
        private HidBatteryAnalyzer _hidBatteryAnalyzer;
        private CalibrationStage _calibrationStage;
        private float actualVoltage1, actualVoltage2;
        private float actualCurrent1, actualCurrent2;

        public CalibrationUI()
        {
            InitializeComponent();

            _hidDevice = new HidInterface(0x1FBD, 0x0003);
            _hidDevice.OnDeviceAttached += new EventHandler(hidPort_OnDeviceAttached);
            _hidDevice.OnDeviceRemoved += new EventHandler(hidPort_OnDeviceRemoved);
            _hidDevice.ConnectTargetDevice();

            _hidBatteryAnalyzer = new HidBatteryAnalyzer(_hidDevice, 50);
            _hidBatteryAnalyzer.OnAnalogDataReceived += _hidBatteryAnalyzer_OnAnalogDataReceived;
            _calibrationStage = CalibrationStage.Reset;
        }

        void _hidBatteryAnalyzer_OnAnalogDataReceived(object sender, AnalogDataReceivedEventArgs e)
        {
            // show voltage constant & offset
            ThreadHelperClass.SetText(this, voltageLabel, _hidBatteryAnalyzer.Voltage.ToString("0.0 V"));
            ThreadHelperClass.SetText(this, voltageConstantlabel, _hidBatteryAnalyzer.VoltageConstant.ToString("0.0000000"));
            ThreadHelperClass.SetText(this, voltageOffsetlabel, _hidBatteryAnalyzer.VoltageOffset.ToString("0.000"));

            // show current constant & offset
            ThreadHelperClass.SetText(this, currentLabel, _hidBatteryAnalyzer.Current.ToString("0 A"));
            ThreadHelperClass.SetText(this, currentConstantLabel, _hidBatteryAnalyzer.CurrentConstant.ToString("0.0000000"));
            ThreadHelperClass.SetText(this, currentOffsetLabel, _hidBatteryAnalyzer.CurrentOffset.ToString("0.000"));
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

        private void resetButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure to RESET device Calibration?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;
            _hidBatteryAnalyzer.ResetCalibration();
            NextCalibrationStage();
        }

        private void setButton_Click(object sender, EventArgs e)
        {
            switch (_calibrationStage)
            {
                case CalibrationStage.Voltage1:
                    if (!float.TryParse(actualValueTextBox.Text, out actualVoltage1))
                    {
                        MessageBox.Show("Enter value in currect format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    actualValueTextBox.Text = "";
                    NextCalibrationStage();
                    break;
                case CalibrationStage.Voltage2:
                    if (!float.TryParse(actualValueTextBox.Text, out actualVoltage2))
                    {
                        MessageBox.Show("Enter value in currect format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    actualValueTextBox.Text = "";
                    NextCalibrationStage();
                    break;
                case CalibrationStage.Current1:
                    if (!float.TryParse(actualValueTextBox.Text, out actualCurrent1))
                    {
                        MessageBox.Show("Enter value in currect format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    actualValueTextBox.Text = "";
                    NextCalibrationStage();
                    break;
                case CalibrationStage.Current2:
                    if (!float.TryParse(actualValueTextBox.Text, out actualCurrent2))
                    {
                        MessageBox.Show("Enter value in currect format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    actualValueTextBox.Text = "";
                    NextCalibrationStage();
                    break;
            }
        }

        private void NextCalibrationStage()
        {
            switch (_calibrationStage)
            {
                case CalibrationStage.Reset:
                    setButton.Text = "Adjust Voltage1 (12 V)";
                    resetButton.Enabled = false;
                    calibrationGroupBox.Enabled = true;
                    _calibrationStage = CalibrationStage.Voltage1;
                    break;
                case CalibrationStage.Voltage1:
                    setButton.Text = "Adjust Voltage2 (24 V)";
                    resetButton.Enabled = false;
                    calibrationGroupBox.Enabled = true;
                    _calibrationStage = CalibrationStage.Voltage2;
                    break;
                case CalibrationStage.Voltage2:
                    setButton.Text = "Adjust Current1 (5 A)";
                    resetButton.Enabled = false;
                    calibrationGroupBox.Enabled = true;
                    _calibrationStage = CalibrationStage.Current1;
                    break;
                case CalibrationStage.Current1:
                    setButton.Text = "Adjust Current2 (20 A)";
                    resetButton.Enabled = false;
                    calibrationGroupBox.Enabled = true;
                    _calibrationStage = CalibrationStage.Current2;
                    break;
                case CalibrationStage.Current2:
                    setButton.Text = "Reset Calibration first";
                    calibrationGroupBox.Enabled = false;
                    resetButton.Enabled = true;
                    _calibrationStage = CalibrationStage.Reset;
                    break;
            }
        }
    }

    enum CalibrationStage
    {
        Reset,
        Voltage1,
        Voltage2,
        Current1,
        Current2
    }
}
