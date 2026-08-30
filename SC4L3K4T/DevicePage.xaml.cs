using SC4L3K4T.Bluetooth;
using SC4L3K4T.Models;
using System.Text;

namespace SC4L3K4T;

public partial class DevicePage : ContentPage
{
    private readonly DeviceControlService _deviceControl;

    public DevicePage(DeviceControlService deviceControl)
	{
		InitializeComponent();

        _deviceControl = deviceControl;

        _deviceControl.DeviceMessageReceived += OnDeviceMessageReceived;

        UpdateInitialState();
    }

    private void UpdateInitialState()
    {
        if (_deviceControl.IsDeviceConnected)
        {
            DeviceStatusLabel.Text = "Estado: Conectado";

            LedButton.IsEnabled = true;
            HeadlightsButton.IsEnabled = true;
        }
        else
        {
            DeviceStatusLabel.Text = "Estado: Desconectado";
            
            LedButton.IsEnabled = false;
            HeadlightsButton.IsEnabled = false;
        }

        if (_deviceControl.IsLedOn.HasValue)
        {
            LedStatusLabel.Text =
                _deviceControl.IsLedOn.Value
                    ? "LED: Encendido"
                    : "LED: Apagado";
        }
        else
        {
            LedStatusLabel.Text =
                "LED: Desconocido";
        }

        if (_deviceControl.AreHeadlightsOn.HasValue)
        {
            HeadlightsButton.Text =
                _deviceControl.AreHeadlightsOn.Value
                    ? "Luces: Encendidas"
                    : "Luces: Apagadas";
        }
        else
        {
            HeadlightsButton.Text =
                "Luces: Desconocidas";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void OnLedButtonPressed(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetLedAsync(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnLedButtonReleased(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetLedAsync(false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnHeadlightsButtonClicked(
    object sender,
    EventArgs e)
    {
        if (!_deviceControl.IsDeviceConnected)
            return;

        try
        {
            var newState =
                !_deviceControl.AreHeadlightsOn.GetValueOrDefault();

            await _deviceControl.SetHeadlightsAsync(newState);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private void OnDeviceMessageReceived(
    object? sender,
    string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (message)
            {
                case "PICO_DISCONNECTED":

                    DeviceStatusLabel.Text =
                        "Estado: Desconectado";

                    LedStatusLabel.Text =
                        "LED: Desconocido";

                    HeadlightsButton.Text =
                        "Luces: Desconocidas";

                    LedButton.IsEnabled = false;
                    HeadlightsButton.IsEnabled = false;

                    break;

                case "PICO_CONNECTED":
                    DeviceStatusLabel.Text = "Pico conectada";

                    LedButton.IsEnabled = true;
                    HeadlightsButton.IsEnabled = true;
                    break;

                case DeviceCommands.LedOn:
                    LedStatusLabel.Text = "LED: Encendido";
                    break;

                case DeviceCommands.LedOff:
                    LedStatusLabel.Text = "LED: Apagado";
                    break;

                case DeviceCommands.HeadlightsOn:

                    HeadlightsButton.Text =
                        "Luces: Encendidas";

                    break;

                case DeviceCommands.HeadlightsOff:

                    HeadlightsButton.Text =
                        "Luces: Apagadas";

                    break;
            }
        });
    }
}