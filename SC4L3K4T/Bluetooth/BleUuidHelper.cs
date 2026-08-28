using System;
using System.Collections.Generic;
using System.Text;

namespace SC4L3K4T.Bluetooth
{
    public static class BleUuidHelper
    {
        public static Guid FromAdvertisementBytes(byte[] data)
        {
            if (data.Length != 16)
                throw new ArgumentException(
                    "Un UUID de 128 bits debe tener exactamente 16 bytes.",
                    nameof(data));

            var guidBytes = new byte[16];

            // BLE transmite los primeros tres campos
            // del UUID en little-endian.
            guidBytes[0] = data[3];
            guidBytes[1] = data[2];
            guidBytes[2] = data[1];
            guidBytes[3] = data[0];

            guidBytes[4] = data[5];
            guidBytes[5] = data[4];

            guidBytes[6] = data[7];
            guidBytes[7] = data[6];

            // Los últimos 8 bytes conservan su orden.
            Array.Copy(
                data,
                8,
                guidBytes,
                8,
                8);

            return new Guid(guidBytes);
        }
    }
}
