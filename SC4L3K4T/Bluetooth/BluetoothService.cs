using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SC4L3K4T.Models;

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

        public async Task<IReadOnlyList<BleDeviceInfo>> ScanAsync()
        {
            if (_bluetooth.State != BluetoothState.On)
            {
                throw new InvalidOperationException(
                    "El Bluetooth está apagado.");
            }

            var devices = new Dictionary<Guid, BleDeviceInfo>();

            void OnDeviceDiscovered(
                object? sender,
                DeviceEventArgs args)
            {
                var device = args.Device;

                var isScaleCar = device.AdvertisementRecords.Any(record =>
                {
                    if (record.Type != AdvertisementRecordType.UuidsComplete128Bit)
                        return false;

                    if (record.Data.Length != 16)
                        return false;

                    var uuid = BleUuidHelper.FromAdvertisementBytes(record.Data);

                    return uuid == BleConstants.ScaleCarServiceUuid;
                });

                if (!isScaleCar)
                    return;

                var deviceInfo = new BleDeviceInfo
                {
                    Device = device
                };

                if (!devices.ContainsKey(device.Id))
                {
                    devices.Add(device.Id, deviceInfo);
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

        public async Task ConnectAsync(BleDeviceInfo deviceInfo)
        {
            await _adapter.ConnectToDeviceAsync(
                deviceInfo.Device,
                new ConnectParameters(
                    autoConnect: false,
                    forceBleTransport: true));
        }

        public async Task<byte[]> ReadAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid)
        {
            var service = await deviceInfo.Device.GetServiceAsync(serviceUuid);

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
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid,
            byte[] data)
        {
            var service = await deviceInfo.Device.GetServiceAsync(serviceUuid);

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

        public async Task DisconnectAsync(BleDeviceInfo deviceInfo)
        {
            await _adapter.DisconnectDeviceAsync(deviceInfo.Device);
        }
    }
}
