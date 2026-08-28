using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace SC4L3K4T.Bluetooth
{
    public class BluetoothService : IBluetoothService
    {
        private readonly IBluetoothLE _bluetooth;
        private readonly IAdapter _adapter;

        public BluetoothService()
        {
            _bluetooth = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
        }

        public async Task<IReadOnlyList<IDevice>> ScanAsync()
        {
            if (_bluetooth.State != BluetoothState.On)
            {
                throw new InvalidOperationException(
                    "El Bluetooth está apagado.");
            }

            var devices = new Dictionary<Guid, IDevice>();

            void OnDeviceDiscovered(
                object? sender,
                DeviceEventArgs args)
            {
                var device = args.Device;

                if (!devices.ContainsKey(device.Id))
                {
                    devices.Add(device.Id, device);
                }
            }

            _adapter.DeviceDiscovered += OnDeviceDiscovered;

            try
            {
                await _adapter.StartScanningForDevicesAsync();
            }
            finally
            {
                _adapter.DeviceDiscovered -= OnDeviceDiscovered;
            }

            return devices.Values.ToList();
        }

        public async Task ConnectAsync(IDevice device)
        {
            await _adapter.ConnectToDeviceAsync(
                device,
                new ConnectParameters(
                    autoConnect: false,
                    forceBleTransport: true));
        }

        public async Task<byte[]> ReadAsync(
            IDevice device,
            Guid serviceUuid,
            Guid characteristicUuid)
        {
            var service = await device.GetServiceAsync(serviceUuid);

            if (service == null)
            {
                throw new InvalidOperationException(
                    "No se encontró el servicio BLE.");
            }

            var characteristic =
                await service.GetCharacteristicAsync(characteristicUuid);

            if (characteristic == null)
            {
                throw new InvalidOperationException(
                    "No se encontró la característica BLE.");
            }

            var (data, resultCode) =
                await characteristic.ReadAsync();

            return data;
        }

        public async Task WriteAsync(
            IDevice device,
            Guid serviceUuid,
            Guid characteristicUuid,
            byte[] data)
        {
            var service = await device.GetServiceAsync(serviceUuid);

            if (service == null)
            {
                throw new InvalidOperationException(
                    "No se encontró el servicio BLE.");
            }

            var characteristic =
                await service.GetCharacteristicAsync(characteristicUuid);

            if (characteristic == null)
            {
                throw new InvalidOperationException(
                    "No se encontró la característica BLE.");
            }

            await characteristic.WriteAsync(data);
        }

        public async Task DisconnectAsync(IDevice device)
        {
            await _adapter.DisconnectDeviceAsync(device);
        }
    }
}
