using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using INFRA.USB;
using INFRA.USB.Classes;
using ZedGraph;

namespace VoltageCurrentGraphApp
{
    public partial class VoltageCurrentGraphUI : Form
    {
        // PointPairList
        RollingPointPairList plVoltage;
        RollingPointPairList plCurrent;

        // Starting time in milliseconds
        int rpmLastTick = 0, rpmTotalTick = 0;
        int vcLastTick = 0, vcTotalTick = 0;

        private HidInterface _hidDevice;
        private HidBatteryAnalyzer _hidBatteryAnalyzer;
        private BackgroundWorker graphDataReader;
        Stopwatch stopWatch = new Stopwatch();

        readonly object _lockObject = new object();

        public VoltageCurrentGraphUI()
        {
            InitializeComponent();

            lock (_lockObject)
            {
                // settings for ZedGraph controls
                InitZedGraphControls();    
            }
            

            _hidDevice = new HidInterface(0x1FBD, 0x0003);
            _hidDevice.OnDeviceAttached += new EventHandler(hidPort_OnDeviceAttached);
            _hidDevice.OnDeviceRemoved += new EventHandler(hidPort_OnDeviceRemoved);
            _hidDevice.ConnectTargetDevice();


            _hidBatteryAnalyzer = new HidBatteryAnalyzer(_hidDevice, 50);
            _hidBatteryAnalyzer.OnAnalogDataReceived += _hidBatteryAnalyzer_OnAnalogDataReceived;

            zgcVoltage.MouseMove += new MouseEventHandler(zgcVoltage_MouseMove);
            zgcCurrent.MouseMove += new MouseEventHandler(zgcCurrent_MouseMove);

            //graphDataReader = new BackgroundWorker();
            //graphDataReader.RunWorkerAsync();
        }

        void _hidBatteryAnalyzer_OnAnalogDataReceived(object sender, AnalogDataReceivedEventArgs e)
        {
            lock (_lockObject)
            {
                try
                {
                    SetGraphDta(e.VoltageData, e.CurrentData);
                    //ScrollGraph();
                    UpdateGraphPan();
                }
                catch {}
            }
        }
        
        private double voltage_x = 0;
        private double current_x = 0;
        private void SetGraphDta(int[] voltageData, int[] currentData)
        {
            foreach (int v in voltageData)
            {
                plVoltage.Add(voltage_x, Math.Abs(v) / 50);
                voltage_x += 0.005;
                if (voltage_x > 200)
                {
                    plVoltage.Clear();
                    voltage_x = 0;
                }
            }

            foreach (int c in currentData)
            {
                plCurrent.Add(current_x, Math.Abs(c)/50);
                current_x += 0.25;
            }
        }

        private void zgcVoltage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lock (_lockObject)
                {
                    zgcCurrent.GraphPane.XAxis.Scale.Max = zgcVoltage.GraphPane.XAxis.Scale.Max;
                    zgcCurrent.GraphPane.XAxis.Scale.Min = zgcVoltage.GraphPane.XAxis.Scale.Min;
                }
            }
        }

        private void zgcCurrent_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lock (_lockObject)
                {
                    zgcVoltage.GraphPane.XAxis.Scale.Max = zgcCurrent.GraphPane.XAxis.Scale.Max;
                    zgcVoltage.GraphPane.XAxis.Scale.Min = zgcCurrent.GraphPane.XAxis.Scale.Min;
                }
            }
        }

        private void InitZedGraphControls()
        {
            // Voltage Graph Specific settiong
            zgcVoltage.GraphPane.AddCurve("Voltage", new RollingPointPairList(250000), Color.Green, SymbolType.None);
            zgcVoltage.GraphPane.YAxis.Scale.FontSpec.FontColor = Color.Green;
            zgcVoltage.GraphPane.YAxis.Title.FontSpec.FontColor = Color.Green;
            zgcVoltage.GraphPane.YAxis.Scale.Min = 0;
            zgcVoltage.GraphPane.YAxis.Scale.Max = 100;
            zgcVoltage.GraphPane.YAxis.Scale.MajorStep = 2;
            zgcVoltage.GraphPane.YAxis.Scale.MinorStep = 0;
            zgcVoltage.GraphPane.Title.Text = " ";//"Voltage vs. Time";
            zgcVoltage.GraphPane.YAxis.Title.Text = "Voltage";
            zgcVoltage.GraphPane.XAxis.Title.Text = "Time";
            zgcVoltage.AxisChange();

            // Current Graph Specific settiong
            zgcCurrent.GraphPane.AddCurve("Current", new RollingPointPairList(1000), Color.Blue, SymbolType.None);
            zgcCurrent.GraphPane.YAxis.Scale.FontSpec.FontColor = Color.Blue;
            zgcCurrent.GraphPane.YAxis.Title.FontSpec.FontColor = Color.Blue;
            zgcCurrent.GraphPane.YAxis.Scale.Min = -100;
            zgcCurrent.GraphPane.YAxis.Scale.Max = 100;
            zgcCurrent.GraphPane.YAxis.Scale.MajorStep = 20;
            zgcCurrent.GraphPane.YAxis.Scale.MinorStep = 0;
            zgcCurrent.GraphPane.Title.Text = " "; //"Current vs. Time";
            zgcCurrent.GraphPane.YAxis.Title.Text = "Current";
            zgcCurrent.GraphPane.XAxis.Title.Text = "Time";
            zgcCurrent.AxisChange();

            // common graph settings
            var zGraphControls = new ZedGraphControl[2];
            zGraphControls[0] = zgcVoltage;
            zGraphControls[1] = zgcCurrent;

            foreach (ZedGraphControl zGraph in zGraphControls)
            {
                // disable cursor tooltip values
                zGraph.IsShowPointValues = false;
                zGraph.IsShowCursorValues = false;

                // scrollbar
                zGraph.IsShowHScrollBar = false;
                zGraph.IsAutoScrollRange = true;
                zGraph.ScrollGrace = 0;

                // enable horizontal zoom and pan
                zGraph.IsEnableHPan = true;
                zGraph.IsEnableHZoom = true;
                zGraph.IsEnableVPan = false;
                zGraph.IsEnableVZoom = false;

                // set X axis
                zGraph.GraphPane.XAxis.Scale.Min = 0;
                zGraph.GraphPane.XAxis.Scale.Max = 200;
                zGraph.GraphPane.XAxis.Scale.MinorStep = 0;
                zGraph.GraphPane.XAxis.Scale.MajorStep = 5;
                zGraph.GraphPane.XAxis.Scale.Align = AlignP.Inside;
                zGraph.GraphPane.XAxis.Scale.FontSpec.Size = 20;
                zGraph.GraphPane.XAxis.Scale.FontSpec.FontColor = Color.Black;
                zGraph.GraphPane.XAxis.MajorGrid.IsVisible = false;
                zGraph.GraphPane.XAxis.MinorGrid.IsVisible = false;
                zGraph.GraphPane.XAxis.Title.FontSpec.FontColor = Color.Black;
                zGraph.GraphPane.XAxis.Title.FontSpec.Size = 16;
                zGraph.GraphPane.XAxis.Title.IsVisible = false;

                zGraph.GraphPane.XAxis.MajorGrid.IsVisible = true;
                zGraph.GraphPane.XAxis.MajorGrid.DashOff = 5;
                zGraph.GraphPane.YAxis.MajorGrid.DashOn = 0;
                //zGraph.GraphPane.XAxis.Scale.IsVisible = false;
                zGraph.GraphPane.XAxis.Scale.IsReverse = true;

                // set Y axis
                zGraph.GraphPane.YAxis.Scale.Align = AlignP.Inside;
                zGraph.GraphPane.YAxis.Scale.FontSpec.Size = 24;
                zGraph.GraphPane.YAxis.MajorGrid.IsVisible = false; //true
                zGraph.GraphPane.YAxis.MajorGrid.DashOff = 5;
                zGraph.GraphPane.YAxis.MajorGrid.DashOn = 0;
                zGraph.GraphPane.YAxis.MajorGrid.IsZeroLine = false;
                zGraph.GraphPane.YAxis.MinorGrid.IsVisible = false;
                zGraph.GraphPane.YAxis.Title.FontSpec.Size = 24;
                zGraph.GraphPane.YAxis.Title.IsVisible = true;

                // hide curve label
                zGraph.GraphPane.CurveList[0].Label.IsVisible = false;

                // title
                zGraph.GraphPane.Title.FontSpec.Size = 10;
                zGraph.GraphPane.Title.IsVisible = true;

                // Scale the axes
                zGraph.AxisChange();
            }

            // Get the PointPairList
            plVoltage = zgcVoltage.GraphPane.CurveList[0].Points as RollingPointPairList;
            plCurrent = zgcCurrent.GraphPane.CurveList[0].Points as RollingPointPairList;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _hidDevice.Dispose();
            base.OnClosing(e);
        }

        void hidPort_OnDeviceRemoved(object sender, EventArgs e)
        {
            ThreadHelperClass.SetText(this, lblUsbConnected, "Not Connected");
        }

        void hidPort_OnDeviceAttached(object sender, EventArgs e)
        {
            ThreadHelperClass.SetText(this, lblUsbConnected, "Connected");
        }

        private void UpdateGraphPan()
        {
            // Make sure the Y axis is rescaled to accommodate actual data
            zgcVoltage.AxisChange();
            zgcCurrent.AxisChange();

            // Force a redraw
            zgcVoltage.Invalidate();
            zgcCurrent.Invalidate();
        }

        private void ScrollGraph()
        {
            var xScale = zgcVoltage.GraphPane.XAxis.Scale;
            double dX = xScale.Max - xScale.Min;

            if (voltage_x > xScale.Max - (dX/100))
            {
                xScale.Max = voltage_x + (dX/100);
                xScale.Min = xScale.Max - dX;
            }

            xScale = zgcCurrent.GraphPane.XAxis.Scale;
            dX = xScale.Max - xScale.Min;
            if (current_x > xScale.Max - (dX/100))
            {
                xScale.Max = current_x + (dX/100);
                xScale.Min = xScale.Max - dX;
            }
        }
    }

}