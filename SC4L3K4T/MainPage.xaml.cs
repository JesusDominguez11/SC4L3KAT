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
        private DeviceControlService? _deviceControlService;

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
                    await RequestEnableBluetoothAsync();
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

        private void ClearConnectionState()
        {
            _isConnected = false;
            _connectedDevice = null;

            if (connectedButton is not null)
            {
                connectedButton.Text = "Conectar";
            }

            if (connectedDevicePageButton is not null)
            {
                connectedDevicePageButton.IsEnabled = false;
            }

            connectedButton = null;
            connectedDevicePageButton = null;
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

                var devicePageButton = new Button
                {
                    Text = "→",
                    FontSize = 24,
                    WidthRequest = 60,
                    IsEnabled = false
                };

                devicePageButton.Clicked += async (_, _) =>
                {
                    if (!_isConnected ||
                        _connectedDevice is null ||
                        _connectedDevice.Id != deviceInfo.Id)
                    {
                        return;
                    }

                    await Navigation.PushAsync(
                        new DevicePage(
                            _deviceControlService));
                };

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

                        _deviceControlService = new DeviceControlService(_bluetoothService, deviceInfo);

                        await _deviceControlService.StartAsync();

                        _isConnected = true;
                        _connectedDevice = deviceInfo;
                        
                        connectedButton = connectButton;
                        connectedDevicePageButton = devicePageButton;
                        
                        connectButton.Text = "Desconectar";

                        devicePageButton.IsEnabled = true;

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

                var buttonsLayout = new HorizontalStackLayout
                {
                    Spacing = 10
                };

                buttonsLayout.Children.Add(connectButton);
                buttonsLayout.Children.Add(devicePageButton);

                deviceLayout.Children.Add(buttonsLayout);
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

        private void SetBluetoothOffState()
        {
            _bluetoothIsOn = false;

            _scanStarted = false;

            ScanActivityIndicator.IsRunning = false;
            ScanActivityIndicator.IsVisible = false;

            BluetoothStatusLabel.Text = "Bluetooth: Apagado";

            UpdateBluetoothSwitch(false);

            DevicesLayout.Clear();
            _discoveredDeviceIds.Clear();
            _deviceRssiLabels.Clear();
        }

        private bool _bluetoothIsOn;
        private void SetBluetoothOnState()
        {
            _bluetoothIsOn = true;

            ScanActivityIndicator.IsRunning = true;
            ScanActivityIndicator.IsVisible = true;

            BluetoothStatusLabel.Text = "Bluetooth: Buscando...";

            UpdateBluetoothSwitch(true);

            DevicesLayout.Clear();
            _discoveredDeviceIds.Clear();
            _deviceRssiLabels.Clear();
        }

        private bool _updatingBluetoothSwitch;
        private void UpdateBluetoothSwitch(bool isOn)
        {
            _updatingBluetoothSwitch = true;

            BluetoothSwitch.IsToggled = isOn;

            _updatingBluetoothSwitch = false;
        }

        private async void OnBluetoothSwitchToggled(
            object? sender,
            ToggledEventArgs e)
        {
            if (_updatingBluetoothSwitch)
                return;

            // Restauramos inmediatamente el estado REAL.
            UpdateBluetoothSwitch(_bluetoothIsOn);

            if (!_bluetoothIsOn)
            {
                // Bluetooth está apagado:
                // solicitar que Android lo active.
                await RequestEnableBluetoothAsync();
            }
            else
            {
                // Bluetooth está encendido:
                // mandar al usuario a los ajustes para apagarlo manualmente.
                await RequestDisableBluetoothAsync();
            }
        }

        private async Task RequestEnableBluetoothAsync()
        {
#if ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                var activity = Platform.CurrentActivity;

                if (activity is null)
                    return;

                var intent = new Android.Content.Intent(
                    Android.Bluetooth.BluetoothAdapter.ActionRequestEnable);

                activity.StartActivity(intent);

                return;
            }
#endif

            await DisplayAlertAsync(
                "Bluetooth",
                "Activa Bluetooth desde la configuración del dispositivo.",
                "OK");
        }

        private async Task RequestDisableBluetoothAsync()
        {
#if ANDROID
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionBluetoothSettings);

                Platform.CurrentActivity?.StartActivity(intent);
            });
#else
    await DisplayAlertAsync(
        "Bluetooth",
        "Desactiva Bluetooth desde la configuración del dispositivo.",
        "OK");
#endif
        }

        private bool _isConnected;
        private BleDeviceInfo? _connectedDevice;

        Button? connectedButton = null;
        Button? connectedDevicePageButton = null;

        //private async void OnLedButtonPressed(object sender, EventArgs e)
        //{
        //    if (!_isConnected || _connectedDevice is null)
        //        return;

        //    var data = System.Text.Encoding.UTF8.GetBytes("LED_ON");

        //    await _bluetoothService.WriteAsync(
        //        _connectedDevice,
        //        BleConstants.ScaleCarServiceUuid,
        //        BleConstants.TestCharacteristicUuid,
        //        data);
        //}

        //private async void OnLedButtonReleased(object sender, EventArgs e)
        //{
        //    if (!_isConnected || _connectedDevice is null)
        //        return;

        //    var data = System.Text.Encoding.UTF8.GetBytes("LED_OFF");

        //    await _bluetoothService.WriteAsync(
        //        _connectedDevice,
        //        BleConstants.ScaleCarServiceUuid,
        //        BleConstants.TestCharacteristicUuid,
        //        data);
        //}
    }
}
