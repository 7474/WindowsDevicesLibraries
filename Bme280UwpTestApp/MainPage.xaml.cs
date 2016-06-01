using Bme280Uwp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Bme280UwpTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        private ABme280 bme280;

        private async void Initialize()
        {
            bme280 = new Bme280I2c("I2C1", 0x76, 1000);
            await bme280.Initialize();
            //SensorValue.Text = bme280.Data.ToString();
            bme280.UpdateSensorData += Bme280i2c_UpdateSensorData;
            bme280.Start();
        }

        private async void Bme280i2c_UpdateSensorData(object sender, Bme280DataUpdateEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SensorValue.Text = e.Data.ToString();
            });
        }
    }
}
