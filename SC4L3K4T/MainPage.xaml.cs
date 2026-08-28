using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

#if ANDROID
using Android;
using Android.Content.PM;
#endif

namespace SC4L3K4T
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
#if ANDROID
    if (OperatingSystem.IsAndroidVersionAtLeast(31))
    {
        var permission = await Permissions.RequestAsync<Permissions.Bluetooth>();

        if (permission != PermissionStatus.Granted)
        {
            await DisplayAlertAsync(
                "Permisos",
                "Se necesita permiso para buscar dispositivos Bluetooth.",
                "OK");

            return;
        }
    }
#endif

            var bluetooth = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;

            if (bluetooth.State != BluetoothState.On)
            {
                await DisplayAlertAsync(
                    "Bluetooth",
                    "El Bluetooth está apagado.",
                    "OK");

                return;
            }

            ScanButton.IsEnabled = false;
            ScanActivityIndicator.IsVisible = true;
            ScanActivityIndicator.IsRunning = true;
            BluetoothStatusLabel.Text = "Bluetooth: Escaneando...";

            var devices = new List<string>();

            adapter.DeviceDiscovered += (s, args) =>
            {
                var device = args.Device;

                var name = string.IsNullOrWhiteSpace(device.Name)
                    ? "Sin nombre"
                    : device.Name;

                var item = $"{name}\n{device.Id}";

                if (!devices.Contains(item))
                {
                    devices.Add(item);
                }
            };

            try
            {
                await adapter.StartScanningForDevicesAsync();
            }
            finally
            {
                ScanActivityIndicator.IsRunning = false;
                ScanActivityIndicator.IsVisible = false;
                ScanButton.IsEnabled = true;
                BluetoothStatusLabel.Text = "Bluetooth: Listo";
            }

            var result = devices.Count == 0
                ? "No se encontraron dispositivos BLE."
                : string.Join("\n\n", devices);

            await DisplayAlertAsync(
                "Dispositivos encontrados",
                result,
                "OK");
        }
    }
}
