using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Models
{
    public class BleDeviceInfo
    {
        public required IDevice Device { get; init; }

        public string Name =>
            string.IsNullOrWhiteSpace(Device.Name)
                ? "SC4L3K4T"
                : Device.Name;

        public Guid Id => Device.Id;

        public int Rssi { get; set; }
    }
}
