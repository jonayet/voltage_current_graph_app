using System;
using System.Collections.Generic;
using System.Text;
using INFRA.USB;

namespace VoltageCurrentGraphApp
{
    public class HidAnalogModule
    {
        private UsbHidPort _hidDevice;

        public HidAnalogModule(UsbHidPort HidDevice)
        {
            _hidDevice = HidDevice;
            //_hidDevice.SpecifiedDevice.
        }
    }
}
