using Plugin.BLE.Abstractions.Contracts;
using SC4L3K4T.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Bluetooth
{
    public interface IBluetoothService
    {
        Task<IReadOnlyList<BleDeviceInfo>> ScanAsync();

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
    }
}
