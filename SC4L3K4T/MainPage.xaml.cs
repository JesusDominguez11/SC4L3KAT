using Plugin.BLE.Abstractions.Contracts;
using SC4L3K4T.Bluetooth;
using SC4L3K4T.Models;



#if ANDROID
using Android;
using Android.Content.PM;
#endif

namespace SC4L3K4T
{
    public partial class MainPage : ContentPage
    {        
        private readonly IBluetoothService _bluetoothService;
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

                foreach (var deviceInfo in devices)
                {
                    await AddDeviceToListAsync(deviceInfo);
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

        private bool _isConnected;
        private BleDeviceInfo? _connectedDevice;
        Button? connectedButton = null;
        private async Task AddDeviceToListAsync(BleDeviceInfo deviceInfo)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var device = deviceInfo.Device;

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
                    if (!_isConnected)
                    {
                        try
                        {
                            BluetoothStatusLabel.Text = $"Conectando a {name}...";

                            await _bluetoothService.ConnectAsync(deviceInfo);

                            _connectedDevice = deviceInfo;
                            _isConnected = true;

                            connectButton.Text = "Desconectar";

                            LedButton.IsEnabled = true;
                            LedButton.Text = "Encender LED";

                            BluetoothStatusLabel.Text = $"Conectado: {name}";

                            //await DisplayAlertAsync(
                            //    "Bluetooth",
                            //    $"Conectado correctamente a:\n{name}",
                            //    "OK");

                            //var data = await _bluetoothService.ReadAsync(
                            //    deviceInfo,
                            //    BleConstants.ScaleCarServiceUuid,
                            //    BleConstants.TestCharacteristicUuid);

                            //var text = System.Text.Encoding.UTF8.GetString(data);

                            //await DisplayAlertAsync(
                            //    "BLE Read",
                            //    $"Datos recibidos:\n{text}",
                            //    "OK");

                            //var message = System.Text.Encoding.UTF8.GetBytes("HELLOWORLD");

                            //await _bluetoothService.WriteAsync(
                            //    deviceInfo,
                            //    BleConstants.ScaleCarServiceUuid,
                            //    BleConstants.TestCharacteristicUuid,
                            //    message);
                        }
                        catch (Exception ex)
                        {
                            BluetoothStatusLabel.Text = "Bluetooth: Error de conexión";

                            await DisplayAlertAsync(
                                "Error",
                                $"{ex.GetType().Name}\n\n{ex.Message}",
                                "OK");
                        }

                        return;
                    }

                    if (_connectedDevice is null)
                        return;

                    try
                    {
                        await _bluetoothService.DisconnectAsync(
                            _connectedDevice);

                        _connectedDevice = null;
                        _isConnected = false;

                        connectButton.Text = "Conectar";

                        LedButton.IsEnabled = false;

                        BluetoothStatusLabel.Text = "Bluetooth: Listo";
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync(
                            "Error al desconectar",
                            ex.Message,
                            "OK");
                    }

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

        private async void OnLedButtonPressed(object sender, EventArgs e)
        {
            if (!_isConnected || _connectedDevice is null)
                return;

            var data = System.Text.Encoding.UTF8.GetBytes("LED_ON");

            await _bluetoothService.WriteAsync(
                _connectedDevice,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }

        private async void OnLedButtonReleased(object sender, EventArgs e)
        {
            if (!_isConnected || _connectedDevice is null)
                return;

            var data = System.Text.Encoding.UTF8.GetBytes("LED_OFF");

            await _bluetoothService.WriteAsync(
                _connectedDevice,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }
    }
}
