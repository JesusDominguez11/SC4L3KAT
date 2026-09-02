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
            ThrottleButton.IsEnabled = true;
        }
        else
        {
            DeviceStatusLabel.Text = "Estado: Desconectado";
            
            LedButton.IsEnabled = false;
            HeadlightsButton.IsEnabled = false;
            ThrottleButton.IsEnabled = false;
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

        LeftSignalButton.IsEnabled = _deviceControl.IsDeviceConnected;

        if (_deviceControl.IsLeftSignalOn.HasValue)
        {
            LeftSignalButton.Text =
                _deviceControl.IsLeftSignalOn.Value
                    ? "Direccional izquierda: Encendida"
                    : "Direccional izquierda: Apagada";
        }
        else
        {
            LeftSignalButton.Text =
                "Direccional izquierda: Desconocida";
        }

        RightSignalButton.IsEnabled =
    _deviceControl.IsDeviceConnected;

        if (_deviceControl.IsRightSignalOn.HasValue)
        {
            RightSignalButton.Text =
                _deviceControl.IsRightSignalOn.Value
                    ? "Direccional derecha: Encendida"
                    : "Direccional derecha: Apagada";
        }
        else
        {
            RightSignalButton.Text =
                "Direccional derecha: Desconocida";
        }

        HazardsButton.IsEnabled =
    _deviceControl.IsDeviceConnected;

        if (_deviceControl.AreHazardsOn.HasValue)
        {
            HazardsButton.Text =
                _deviceControl.AreHazardsOn.Value
                    ? "Intermitentes: Encendidas"
                    : "Intermitentes: Apagadas";
        }
        else
        {
            HazardsButton.Text =
                "Intermitentes: Desconocidas";
        }

        BrakeButton.IsEnabled =
    _deviceControl.IsDeviceConnected;

        if (_deviceControl.IsBrakeOn.HasValue)
        {
            BrakeButton.Text =
                _deviceControl.IsBrakeOn.Value
                    ? "Freno: Activado"
                    : "Mantener para frenar";
        }
        else
        {
            BrakeButton.Text =
                "Freno: Desconocido";
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

    private async void OnLeftSignalButtonClicked(
    object sender,
    EventArgs e)
    {
        if (!_deviceControl.IsDeviceConnected)
            return;

        try
        {
            var newState =
                !_deviceControl.IsLeftSignalOn.GetValueOrDefault();

            await _deviceControl.SetLeftSignalAsync(newState);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnRightSignalButtonClicked(
    object sender,
    EventArgs e)
    {
        if (!_deviceControl.IsDeviceConnected)
            return;

        try
        {
            var newState =
                !_deviceControl.IsRightSignalOn.GetValueOrDefault();

            await _deviceControl.SetRightSignalAsync(newState);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnHazardsButtonClicked(
    object sender,
    EventArgs e)
    {
        if (!_deviceControl.IsDeviceConnected)
            return;

        try
        {
            var newState =
                !_deviceControl.AreHazardsOn.GetValueOrDefault();

            await _deviceControl.SetHazardsAsync(newState);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnBrakeButtonPressed(
    object sender,
    EventArgs e)
    {
        try
        {
            await _deviceControl.SetBrakeAsync(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }


    private async void OnBrakeButtonReleased(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetBrakeAsync(false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }


private async void OnLeftSignalTapped(
    object sender,
    EventArgs e)
    {
        try
        {
            await _deviceControl.SetLeftSignalAsync(
                !_deviceControl.IsLeftSignalOn ?? false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }


    private async void OnRightSignalTapped(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetRightSignalAsync(
                !_deviceControl.IsRightSignalOn ?? false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }


    private async void OnHazardsTapped(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetHazardsAsync(
                !_deviceControl.AreHazardsOn ?? false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnIgnitionButtonPressed(
    object sender,
    EventArgs e)
    {
        try
        {
            await _deviceControl.SetIgnitionAsync(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnIgnitionButtonReleased(
    object sender,
    EventArgs e)
    {
        try
        {
            await _deviceControl.SetIgnitionAsync(false);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

private async void OnThrottleButtonPressed(
    object sender,
    EventArgs e)
    {
        try
        {
            await _deviceControl.SetThrottleAsync(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnThrottleButtonReleased(
        object sender,
        EventArgs e)
    {
        try
        {
            await _deviceControl.SetThrottleAsync(false);
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

                    LeftSignalButton.Text =
                        "Direccional izquierda: Desconocida";

                    LeftSignalButton.IsEnabled = false;

                    RightSignalButton.Text =
    "Direccional derecha: Desconocida";

                    RightSignalButton.IsEnabled = false;

                    HazardsButton.Text =
    "Intermitentes: Desconocidas";

                    HazardsButton.IsEnabled = false;

                    BrakeButton.IsEnabled = false;

                    BrakeButton.Text =
                        "Freno: Desconocido";

                    ThrottleButton.IsEnabled = false;

                    break;

                case "PICO_CONNECTED":
                    DeviceStatusLabel.Text = "Pico conectada";

                    LedButton.IsEnabled = true;
                    HeadlightsButton.IsEnabled = true;
                    LeftSignalButton.IsEnabled = true;
                    RightSignalButton.IsEnabled = true;
                    HazardsButton.IsEnabled = true;
                    BrakeButton.IsEnabled = true;
                    ThrottleButton.IsEnabled = true;
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

                case DeviceCommands.LeftSignalOn:

                    LeftSignalButton.Text =
                        "Direccional izquierda: Encendida";

                    break;

                case DeviceCommands.LeftSignalOff:

                    LeftSignalButton.Text =
                        "Direccional izquierda: Apagada";

                    break;

                case DeviceCommands.RightSignalOn:

                    RightSignalButton.Text =
                        "Direccional derecha: Encendida";

                    break;

                case DeviceCommands.RightSignalOff:

                    RightSignalButton.Text =
                        "Direccional derecha: Apagada";

                    break;

                case DeviceCommands.HazardsOn:

                    HazardsButton.Text =
                        "Intermitentes: Encendidas";

                    break;

                case DeviceCommands.HazardsOff:

                    HazardsButton.Text =
                        "Intermitentes: Apagadas";

                    break;

                case DeviceCommands.BrakeOn:

                    BrakeButton.Text =
                        "Freno: Activado";

                    break;

                case DeviceCommands.BrakeOff:

                    BrakeButton.Text =
                        "Mantener para frenar";

                    break;
            }
        });
    }
}