using Plugin.BLE.Abstractions.Contracts;
using SC4L3K4T.Bluetooth;
using SC4L3K4T.Models;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE;





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
            _bluetoothService.DeviceDiscovered += OnDeviceDiscovered;

            _bluetoothService.BluetoothStateChanged += OnBluetoothStateChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
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

                if (CrossBluetoothLE.Current.State == BluetoothState.On)
                {
                    SetBluetoothOnState();
                    _ = StartBluetoothScanAsync();
                }
                else
                {
                    SetBluetoothOffState();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Bluetooth",
                    ex.Message,
                    "OK");
            }
        }

        private void SetBluetoothOffState()
        {
            _scanStarted = false;

            ScanActivityIndicator.IsRunning = false;
            ScanActivityIndicator.IsVisible = false;

            BluetoothStatusLabel.Text = "Bluetooth: Apagado";

            DevicesLayout.Clear();
            _discoveredDeviceIds.Clear();
            _deviceRssiLabels.Clear();
        }

        private void SetBluetoothOnState()
        {
            ScanActivityIndicator.IsRunning = true;
            ScanActivityIndicator.IsVisible = true;

            BluetoothStatusLabel.Text = "Bluetooth: Buscando...";

            DevicesLayout.Clear();
            _discoveredDeviceIds.Clear();
            _deviceRssiLabels.Clear();
        }

        private void ClearConnectionState()
        {
            _isConnected = false;
            _connectedDevice = null;

            if (connectedButton is not null)
            {
                connectedButton.Text = "Conectar";
            }

            connectedButton = null;
        }

        private async Task StartBluetoothScanAsync()
        {
            if (_scanStarted)
                return;

            _scanStarted = true;

            try
            {
                await _bluetoothService.ScanAsync();
            }
            catch (Exception ex)
            {
                _scanStarted = false;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync(
                        "Bluetooth",
                        ex.Message,
                        "OK");
                });
            }
        }

        private readonly HashSet<Guid> _discoveredDeviceIds = new();
        private readonly Dictionary<Guid, Label> _deviceRssiLabels = new();
        private async void OnDeviceDiscovered(
    object? sender,
    BleDeviceInfo deviceInfo)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // ========================================
                // Dispositivo ya existente
                // ========================================

                if (_discoveredDeviceIds.Contains(deviceInfo.Id))
                {
                    if (_deviceRssiLabels.TryGetValue(
                        deviceInfo.Id,
                        out var rssiLabel))
                    {
                        rssiLabel.Text = $"Señal: {deviceInfo.Rssi} dBm";
                    }

                    return;
                }

                // ========================================
                // Dispositivo nuevo
                // ========================================

                _discoveredDeviceIds.Add(deviceInfo.Id);

                var nameLabel = new Label
                {
                    Text = deviceInfo.Name,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold
                };

                var idLabel = new Label
                {
                    Text = deviceInfo.Id.ToString(),
                    FontSize = 12
                };

                var rssiLabelNew = new Label
                {
                    Text = $"Señal: {deviceInfo.Rssi} dBm",
                    FontSize = 14
                };

                _deviceRssiLabels.Add(
    deviceInfo.Id,
    rssiLabelNew);

                var connectButton = new Button
                {
                    Text = "Conectar",
                    BackgroundColor = Colors.White
                };

                connectButton.Clicked += async (_, _) =>
                {
                    try
                    {
                        if (_isConnected &&
                            _connectedDevice is not null &&
                            _connectedDevice.Id == deviceInfo.Id)
                        {
                            await _bluetoothService.DisconnectAsync(
                                _connectedDevice);

                            ClearConnectionState();

                            connectButton.Text = "Conectar";

                            return;
                        }

                        if (_isConnected && _connectedDevice is not null)
                        {
                            if (_connectedDevice.Id != deviceInfo.Id)
                            {
                                await _bluetoothService.DisconnectAsync(
                                    _connectedDevice);

                                ClearConnectionState();
                            }
                        }

                        await _bluetoothService.ConnectAsync(deviceInfo);

                        _isConnected = true;
                        _connectedDevice = deviceInfo;
                        connectedButton = connectButton;

                        connectButton.Text = "Desconectar";

                        await DisplayAlertAsync(
                            "Conectado",
                            $"Conectado a {deviceInfo.Name}",
                            "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync(
                            "Error de conexión",
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
                deviceLayout.Children.Add(rssiLabelNew);
                deviceLayout.Children.Add(connectButton);

                DevicesLayout.Children.Add(deviceLayout);
            });
        }

        private bool _scanStarted;
        private async void OnBluetoothStateChanged(
    object? sender,
    BluetoothStateChangedArgs e)
        {
            if (e.NewState == BluetoothState.On)
            {
                SetBluetoothOnState();
                await StartBluetoothScanAsync();
            }
            else
            {
                ClearConnectionState();
                SetBluetoothOffState();
            }
        }

        private bool _isConnected;
        private BleDeviceInfo? _connectedDevice;
        Button? connectedButton = null;
    
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
