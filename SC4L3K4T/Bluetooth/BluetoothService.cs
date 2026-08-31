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
        public event EventHandler<BleDeviceInfo>? DeviceDiscovered;
        public event EventHandler<BluetoothStateChangedArgs>? BluetoothStateChanged;
        private BleDeviceInfo? _connectedDevice;
        public event EventHandler<BleDeviceInfo>? DeviceDisconnected;
        private bool _isScanning;
        private readonly Dictionary<Guid, EventHandler<CharacteristicUpdatedEventArgs>> _notificationHandlers = new();

        public BluetoothService()
        {
            _bluetooth = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            _bluetooth.StateChanged += OnBluetoothStateChanged;
            _adapter.DeviceDisconnected += OnDeviceDisconnected;
            _adapter.DeviceConnectionLost += OnDeviceDisconnected;
        }

        public async Task ScanAsync()
        {
            if (_bluetooth.State != BluetoothState.On)
            {
                throw new InvalidOperationException(
                    "El Bluetooth está apagado.");
            }

            if (_isScanning)
                return;

            _isScanning = true;

            void OnDeviceDiscovered(object? sender, DeviceEventArgs args)
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
                    Device = device,
                    Rssi = device.Rssi
                };

                DeviceDiscovered?.Invoke(
                    this,
                    deviceInfo);
            }

            _adapter.DeviceDiscovered += OnDeviceDiscovered;

            try
            {
                while (_isScanning && _bluetooth.State == BluetoothState.On)
                {
                    await _adapter.StartScanningForDevicesAsync();
                }
            }
            finally
            {
                _adapter.DeviceDiscovered -= OnDeviceDiscovered;
                _isScanning = false;
            }
        }

        private void OnDeviceDisconnected(object? sender, DeviceEventArgs e)
        {
            var deviceInfo = new BleDeviceInfo
            {
                Device = e.Device
            };

            NotifyDeviceDisconnected(deviceInfo);
        }

        private void OnBluetoothStateChanged(object? sender, BluetoothStateChangedArgs e)
        {
            if (e.NewState != BluetoothState.On)
            {
                _isScanning = false;

                if (_connectedDevice is not null)
                {
                    NotifyDeviceDisconnected(_connectedDevice);
                }
            }

            BluetoothStateChanged?.Invoke(
                this,
                e);
        }

        public async Task ConnectAsync(BleDeviceInfo deviceInfo)
        {
            if (_connectedDevice is not null &&
                _connectedDevice.Id != deviceInfo.Id)
            {
                var previousDevice = _connectedDevice;

                await _adapter.DisconnectDeviceAsync(
                    previousDevice.Device);

                NotifyDeviceDisconnected(previousDevice);
            }

            await _adapter.ConnectToDeviceAsync(
                deviceInfo.Device,
                new ConnectParameters(
                    autoConnect: false,
                    forceBleTransport: true));

            _connectedDevice = deviceInfo;
        }

        public async Task<byte[]> ReadAsync(BleDeviceInfo deviceInfo, Guid serviceUuid, Guid characteristicUuid)
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

        public async Task WriteAsync(BleDeviceInfo deviceInfo, Guid serviceUuid, Guid characteristicUuid, byte[] data)
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
            await _adapter.DisconnectDeviceAsync(
                deviceInfo.Device);

            NotifyDeviceDisconnected(deviceInfo);
        }

        public async Task StartNotificationsAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid,
            Action<byte[]> onDataReceived)
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

            if (_notificationHandlers.ContainsKey(characteristic.Id))
                return;

            EventHandler<CharacteristicUpdatedEventArgs> handler =
                (_, args) =>
                {
                    onDataReceived(args.Characteristic.Value);
                };

            _notificationHandlers.Add(
                characteristic.Id,
                handler);

            characteristic.ValueUpdated += handler;

            await characteristic.StartUpdatesAsync();
        }

        public async Task StopNotificationsAsync(BleDeviceInfo deviceInfo, Guid serviceUuid, Guid characteristicUuid)
        {
            var service = await deviceInfo.Device.GetServiceAsync(serviceUuid);

            if (service == null)
                return;

            var characteristic =
                await service.GetCharacteristicAsync(characteristicUuid);

            if (characteristic == null)
                return;

            if (_notificationHandlers.TryGetValue(
                characteristic.Id,
                out var handler))
            {
                characteristic.ValueUpdated -= handler;

                _notificationHandlers.Remove(
                    characteristic.Id);
            }

            await characteristic.StopUpdatesAsync();
        }

        private void NotifyDeviceDisconnected(BleDeviceInfo deviceInfo)
        {
            if (_connectedDevice?.Id != deviceInfo.Id)
                return;

            _connectedDevice = null;

            DeviceDisconnected?.Invoke(
                this,
                deviceInfo);
        }
    }
}
