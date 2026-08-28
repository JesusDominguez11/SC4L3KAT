using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;


#if ANDROID
using Android;
using Android.Content.PM;
#endif

namespace SC4L3K4T
{
    public partial class MainPage : ContentPage
    {
        private IDevice? _connectedDevice;
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

            DevicesLayout.Clear();

            var devices = new Dictionary<string, IDevice>();

            adapter.DeviceDiscovered += OnDeviceDiscovered;

            try
            {
                await adapter.StartScanningForDevicesAsync();
            }
            finally
            {
                adapter.DeviceDiscovered -= OnDeviceDiscovered;

                ScanActivityIndicator.IsRunning = false;
                ScanActivityIndicator.IsVisible = false;

                ScanButton.IsEnabled = true;

                BluetoothStatusLabel.Text = $"Bluetooth: Listo ({devices.Count} dispositivos)";
            }

            async void OnDeviceDiscovered(object? sender, DeviceEventArgs args)
            {
                var device = args.Device;

                if (devices.ContainsKey(device.Id.ToString()))
                    return;

                if (string.IsNullOrWhiteSpace(device.Name)) //quitar esto al hacer pruebas con el UUID
                {
                    return;
                }

                devices.Add(device.Id.ToString(), device);

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
                        BackgroundColor=Colors.White
                    };

                    connectButton.Clicked += async (_, _) =>
                    {
                        try
                        {
                            BluetoothStatusLabel.Text = $"Conectando a {name}...";

                            await adapter.ConnectToDeviceAsync(device);

                            _connectedDevice = device;

                            BluetoothStatusLabel.Text = $"Conectado: {name}";

                            await DisplayAlertAsync(
                                "Bluetooth",
                                $"Conectado correctamente a:\n{name}",
                                "OK");
                        }
                        catch (Exception ex)
                        {
                            BluetoothStatusLabel.Text = "Bluetooth: Error de conexión";

                            await DisplayAlertAsync(
                                "Error",
                                $"No se pudo conectar:\n{ex.Message}",
                                "OK");
                        }
                    };

                    var deviceLayout = new VerticalStackLayout
                    {
                        Padding = 15,
                        Spacing = 5,
                        BackgroundColor=Colors.DarkSlateGray
                    };

                    deviceLayout.Children.Add(nameLabel);
                    deviceLayout.Children.Add(idLabel);
                    deviceLayout.Children.Add(connectButton);

                    DevicesLayout.Children.Add(deviceLayout);
                });
            }
        }
    }
}
