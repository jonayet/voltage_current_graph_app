using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using INFRA.USB;
using VoltageCurrentGraphApp.VoltageCurrentGraphApp;
using ZedGraph;

namespace VoltageCurrentGraphApp
{
    public partial class VoltageCurrentGraphUI : Form
    {
        // PointPairList
        IPointListEdit plVoltage;
        IPointListEdit plCurrent;

        // Starting time in milliseconds
        int rpmLastTick = 0, rpmTotalTick = 0;
        int vcLastTick = 0, vcTotalTick = 0;

        private UsbHidPort hidPort;
        RingBuffer<int> _voltageBuffer;
        RingBuffer<int> _currentBuffer;
        private Oscilloscope _oscilloscope;

        private BackgroundWorker graphDataReader;

        Stopwatch stopWatch = new Stopwatch();

        public VoltageCurrentGraphUI()
        {
            InitializeComponent();

            _voltageBuffer = new RingBuffer<int>(50000);
            _currentBuffer = new RingBuffer<int>(5000);

            // settings for ZedGraph controls
            InitZedGraphControls();

            hidPort = new UsbHidPort(0x1FBD, 0x0003);
            hidPort.OnDeviceAttached += new EventHandler(hidPort_OnDeviceAttached);
            hidPort.OnDeviceRemoved += new EventHandler(hidPort_OnDeviceRemoved);
            hidPort.OnDataRecieved += new DataRecievedEventHandler(hidPort_OnDataRecieved);
            hidPort.CheckDevicePresent();

            zgcVoltage.MouseMove += new MouseEventHandler(zgcVoltage_MouseMove);
            zgcCurrent.MouseMove += new MouseEventHandler(zgcCurrent_MouseMove);

            graphDataReader = new BackgroundWorker();
            graphDataReader.DoWork += new DoWorkEventHandler(graphDataReader_DoWork);
            graphDataReader.RunWorkerAsync();
        }

        //private bool IsStopWatchStarted = false;
        //private int tmr = 0;
        private void hidPort_OnDataRecieved(object sender, DataRecievedEventArgs args)
        {
            int[] voltageReadeings = GetIntArray(args.data, 1, 50);
            int[] currentReadeings = GetIntArray(args.data, 51, 2);
            _voltageBuffer.PutBlocking(voltageReadeings, 0, voltageReadeings.Length);
            _currentBuffer.PutBlocking(currentReadeings, 0, currentReadeings.Length);

            /*
            if (tmr++%100 == 0)
            {
                if (!IsStopWatchStarted)
                {
                    IsStopWatchStarted = true;
                    stopWatch.Reset();
                    stopWatch.Start();
                }
                else
                {
                    stopWatch.Stop();
                    ThreadHelperClass.SetText(this, voltageLabel, voltageReadeings[0].ToString());
                    ThreadHelperClass.SetText(this, currentLabel, currentReadeings[0].ToString());
                    ThreadHelperClass.SetText(this, label1, (stopWatch.Elapsed.TotalMilliseconds / 100).ToString());
                    ThreadHelperClass.SetText(this, label2, "R.V: " + _voltageBuffer.RemainingLengthToWrite);
                    ThreadHelperClass.SetText(this, label3, "R.C: " + _currentBuffer.RemainingLengthToWrite);
                    IsStopWatchStarted = false;
                }
            }
            */
        }

        void graphDataReader_DoWork(object sender, DoWorkEventArgs e)
        {
            int[] voltageData = new int[5000];
            int[] currentData = new int[200];
            while (true)
            {
                //stopWatch.Reset();
                //stopWatch.Start();
                _voltageBuffer.GetBlocking(voltageData, voltageData.Length);
                _currentBuffer.GetBlocking(currentData, currentData.Length);


                SetGraphDta(voltageData, currentData);

                // Osciloscope-Lib
                //int c = 0, i = 0, j = 0;
                //foreach (int v in voltageData)
                //{
                //    if (i++ % 25 == 0) { c = currentData[j++]; }
                //    _oscilloscope.AddData(v * 1.0, c * 1.0, 0);
                //}

                System.Threading.Thread.Sleep(100);
                //stopWatch.Stop();
                //ThreadHelperClass.SetText(this, label1, (stopWatch.Elapsed.TotalMilliseconds).ToString());
            }
        }

        int[] GetIntArray(byte[] bytes, int startIndex, int length)
        {
            var result = new int[length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToInt16(bytes, startIndex + (i * 2));
            }
            return result;
        }

        private double voltage_x = 0;
        private double current_x = 0;
        private void SetGraphDta(int[] voltageData, int[] currentData)
        {
            foreach (int v in voltageData)
            {
                plVoltage.Add(voltage_x, Math.Abs(v) / 50);
                voltage_x += 0.01;
            }

            foreach (int c in currentData)
            {
                plCurrent.Add(current_x, Math.Abs(c) / 50);
                current_x += 0.25;
            }
        }

        void zgcVoltage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                zgcCurrent.GraphPane.XAxis.Scale.Max = zgcVoltage.GraphPane.XAxis.Scale.Max;
                zgcCurrent.GraphPane.XAxis.Scale.Min = zgcVoltage.GraphPane.XAxis.Scale.Min;
            }
        }

        void zgcCurrent_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                zgcVoltage.GraphPane.XAxis.Scale.Max = zgcCurrent.GraphPane.XAxis.Scale.Max;
                zgcVoltage.GraphPane.XAxis.Scale.Min = zgcCurrent.GraphPane.XAxis.Scale.Min;
            }
        }

        private void InitZedGraphControls()
        {
            // Voltage Graph Specific settiong
            zgcVoltage.GraphPane.AddCurve("Voltage", new RollingPointPairList(10000), Color.Green, SymbolType.None);
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
            zgcCurrent.GraphPane.AddCurve("Current", new RollingPointPairList(400), Color.Blue, SymbolType.None);
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
            ZedGraphControl[] zGraphControls = new ZedGraphControl[2];
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
                zGraph.GraphPane.XAxis.Scale.Min = -30;
                zGraph.GraphPane.XAxis.Scale.Max = 0;
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

                // set Y axis
                zGraph.GraphPane.YAxis.Scale.Align = AlignP.Inside;
                zGraph.GraphPane.YAxis.Scale.FontSpec.Size = 24;
                zGraph.GraphPane.YAxis.MajorGrid.IsVisible = true;
                zGraph.GraphPane.YAxis.MajorGrid.DashOff = 10;
                zGraph.GraphPane.YAxis.MajorGrid.DashOn = 1;
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
            plVoltage = zgcVoltage.GraphPane.CurveList[0].Points as IPointListEdit;
            plCurrent = zgcCurrent.GraphPane.CurveList[0].Points as IPointListEdit;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            hidPort.Dispose();
            base.OnClosing(e);
        }

        void hidPort_OnDeviceRemoved(object sender, EventArgs e)
        {
            lblUsbConnected.Text = "Not Connected";
        }

        void hidPort_OnDeviceAttached(object sender, EventArgs e)
        {
            lblUsbConnected.Text = "Connected";
        }

        protected override void WndProc(ref Message m)
        {
            if (hidPort != null)
                hidPort.ParseMessages(ref m);
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (hidPort != null)
                hidPort.RegisterHandle(Handle);
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (hidPort != null)
                hidPort.UnregisterHandle();
            base.OnHandleDestroyed(e);
        }

        private void tmrGraphUpdater_Tick(object sender, EventArgs e)
        {
            // Make sure the Y axis is rescaled to accommodate actual data
            //zgcVoltage.AxisChange();
            //zgcCurrent.AxisChange();

            // Force a redraw
            zgcVoltage.Invalidate();
            zgcCurrent.Invalidate();
        }

        private void tmrGraphScroller_Tick(object sender, EventArgs e)
        {
            Scale xScale = zgcVoltage.GraphPane.XAxis.Scale;
            double dX = xScale.Max - xScale.Min;
            if (voltage_x > xScale.Max - (dX / 100))
            {
                xScale.Max = voltage_x + (dX / 100);
                xScale.Min = xScale.Max - dX;
            }

            xScale = zgcCurrent.GraphPane.XAxis.Scale;
            dX = xScale.Max - xScale.Min;
            if (current_x > xScale.Max - (dX / 100))
            {
                xScale.Max = current_x + (dX / 100);
                xScale.Min = xScale.Max - dX;
            }
        }
    }
}