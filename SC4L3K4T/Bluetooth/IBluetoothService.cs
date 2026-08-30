using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SC4L3K4T.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Bluetooth
{
    public interface IBluetoothService
    {
        event EventHandler<BleDeviceInfo>? DeviceDiscovered;
        event EventHandler<BluetoothStateChangedArgs>? BluetoothStateChanged;

        Task ScanAsync();

        Task ConnectAsync(BleDeviceInfo deviceInfo);

        Task<byte[]> ReadAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid);

        Task WriteAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid,
            byte[] data);

        Task DisconnectAsync(BleDeviceInfo deviceInfo);

        Task StartNotificationsAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid,
            Action<byte[]> onDataReceived);

        Task StopNotificationsAsync(
            BleDeviceInfo deviceInfo,
            Guid serviceUuid,
            Guid characteristicUuid);
    }
}
