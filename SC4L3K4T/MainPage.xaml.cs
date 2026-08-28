using Plugin.BLE.Abstractions.Contracts;
using SC4L3K4T.Bluetooth;


#if ANDROID
using Android;
using Android.Content.PM;
#endif

namespace SC4L3K4T
{
    public partial class MainPage : ContentPage
    {
        private IDevice? _connectedDevice;
        private readonly IBluetoothService _bluetoothService;
        private static readonly Guid ScaleCarServiceUuid = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
        private static readonly Guid TestCharacteristicUuid = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        public MainPage()
        {
            InitializeComponent();

            _bluetoothService = new BluetoothService();
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

            ScanButton.IsEnabled = false;
            ScanActivityIndicator.IsVisible = true;
            ScanActivityIndicator.IsRunning = true;

            BluetoothStatusLabel.Text = "Bluetooth: Escaneando...";

            DevicesLayout.Clear();

            try
            {
                var devices = await _bluetoothService.ScanAsync();

                foreach (var device in devices)
                {
                    await AddDeviceToListAsync(device);
                }

                BluetoothStatusLabel.Text =
                    $"Bluetooth: Listo ({devices.Count} dispositivos)";
            }
            catch (Exception ex)
            {
                BluetoothStatusLabel.Text = "Bluetooth: Error";

                await DisplayAlertAsync(
                    "Bluetooth",
                    ex.Message,
                    "OK");
            }
            finally
            {
                ScanActivityIndicator.IsRunning = false;
                ScanActivityIndicator.IsVisible = false;

                ScanButton.IsEnabled = true;
            }            
        }

        private async Task AddDeviceToListAsync(IDevice device)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var name = string.IsNullOrWhiteSpace(device.Name)
                    ? "Sin nombre"
                    : device.Name;

                var nameLabel = new Label
                {
                    Text = name,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold
                };

                var idLabel = new Label
                {
                    Text = device.Id.ToString(),
                    FontSize = 12
                };

                var connectButton = new Button
                {
                    Text = "Conectar",
                    BackgroundColor = Colors.White
                };

                connectButton.Clicked += async (_, _) =>
                {
                    await DisplayAlertAsync(
                        "Dispositivo seleccionado",
                        $"{name}\n{device.Id}",
                        "OK");
                };

                var deviceLayout = new VerticalStackLayout
                {
                    Padding = 15,
                    Spacing = 5,
                    BackgroundColor = Colors.DarkSlateGray
                };

                deviceLayout.Children.Add(nameLabel);
                deviceLayout.Children.Add(idLabel);
                deviceLayout.Children.Add(connectButton);

                DevicesLayout.Children.Add(deviceLayout);
            });
        }
    }
}
