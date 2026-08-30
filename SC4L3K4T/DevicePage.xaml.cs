using SC4L3K4T.Bluetooth;
using SC4L3K4T.Models;
using System.Text;

namespace SC4L3K4T;

public partial class DevicePage : ContentPage
{
    private readonly IBluetoothService _bluetoothService;
    private readonly BleDeviceInfo _device;

    public DevicePage(IBluetoothService bluetoothService,
        BleDeviceInfo device)
	{
		InitializeComponent();

        _bluetoothService = bluetoothService;
        _device = device;

        DeviceNameLabel.Text = _device.Name;
        DeviceIdLabel.Text = _device.Id.ToString();
    }

    private async void OnLedButtonPressed(
        object sender,
        EventArgs e)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes("LED_ON");

            await _bluetoothService.WriteAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
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
            var data = Encoding.UTF8.GetBytes("LED_OFF");

            await _bluetoothService.WriteAsync(
                _device,
                BleConstants.ScaleCarServiceUuid,
                BleConstants.TestCharacteristicUuid,
                data);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }
}