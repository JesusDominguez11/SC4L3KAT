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
        if (_deviceControl.LastMessage == "PICO_CONNECTED")
        {
            DeviceStatusLabel.Text = "Estado: Conectado";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //try
        //{
        //    await _deviceControl.StartAsync();
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlertAsync(
        //        "Bluetooth",
        //        ex.Message,
        //        "OK");
        //}
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        //try
        //{
        //    await _deviceControl.StopAsync();
        //}
        //catch
        //{
        //    // El dispositivo puede haberse desconectado
        //    // mientras salíamos de la página.
        //}
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

    private void OnDeviceMessageReceived(
    object? sender,
    string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (message)
            {
                case "PICO_CONNECTED":
                    DeviceStatusLabel.Text = "Pico conectada";
                    break;

                case "LED_ON":
                    LedStatusLabel.Text = "LED: Encendido";
                    break;

                case "LED_OFF":
                    LedStatusLabel.Text = "LED: Apagado";
                    break;
            }
        });
    }
}