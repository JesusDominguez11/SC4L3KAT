using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Bluetooth
{
    public interface IBluetoothService
    {
        Task<IReadOnlyList<IDevice>> ScanAsync();

        Task ConnectAsync(IDevice device);

        Task<byte[]> ReadAsync(
            IDevice device,
            Guid serviceUuid,
            Guid characteristicUuid);

        Task WriteAsync(
            IDevice device,
            Guid serviceUuid,
            Guid characteristicUuid,
            byte[] data);

        Task DisconnectAsync(IDevice device);
    }
}
