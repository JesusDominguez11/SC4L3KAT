using SC4L3K4T.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Bluetooth
{
    public class DeviceControlService
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly BleDeviceInfo _device;

        public DeviceControlService(
            IBluetoothService bluetoothService,
            BleDeviceInfo device)
        {
            _bluetoothService = bluetoothService;
            _device = device;
        }

        public async Task SetLedAsync(bool isOn)
        {
            var command = isOn
                ? "LED_ON"
                : "LED_OFF";

            var data = Encoding.UTF8.GetBytes(command);

            await _bluetoothService.WriteAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }
    }
}
