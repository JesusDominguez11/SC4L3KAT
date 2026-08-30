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

        public event EventHandler<string>? DeviceMessageReceived;

        public DeviceControlService(
            IBluetoothService bluetoothService,
            BleDeviceInfo device)
        {
            _bluetoothService = bluetoothService;
            _device = device;

            _bluetoothService.DeviceDisconnected += OnDeviceDisconnected;
        }

        public async Task StartAsync()
        {
            await _bluetoothService.StartNotificationsAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                OnDataReceived);

            IsDeviceConnected = true;

            DeviceMessageReceived?.Invoke(
                this,
                "PICO_CONNECTED");

            var data = Encoding.UTF8.GetBytes(
                DeviceCommands.Subscribe);

            await _bluetoothService.WriteAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }

        public async Task StopAsync()
        {
            await _bluetoothService.StopNotificationsAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid);
        }

        public string? LastMessage { get; private set; }
        public bool? IsLedOn { get; private set; }
        public bool IsDeviceConnected { get; private set; }

        private void OnDataReceived(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);

            LastMessage = message;

            switch (message)
            {
                case "PICO_CONNECTED":
                    IsDeviceConnected = true;
                    break;

                case DeviceCommands.LedOn:
                    IsLedOn = true;
                    break;

                case DeviceCommands.LedOff:
                    IsLedOn = false;
                    break;
            }

            DeviceMessageReceived?.Invoke(
                this,
                message);
        }

        public async Task SetLedAsync(bool isOn)
        {
            if (!IsDeviceConnected)
                return;

            var command = isOn
                ? DeviceCommands.LedOn
                : DeviceCommands.LedOff;

            var data = Encoding.UTF8.GetBytes(command);

            await _bluetoothService.WriteAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }

        private void OnDeviceDisconnected(
    object? sender,
    BleDeviceInfo deviceInfo)
        {
            if (deviceInfo.Id != _device.Id)
                return;

            IsDeviceConnected = false;

            DeviceMessageReceived?.Invoke(
                this,
                "PICO_DISCONNECTED");
        }
    }
}
