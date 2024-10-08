using Iot.Device.Ssd13xx;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using NanoTemp.Data;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace NanoTemp
{
    public class Program
    {
        private static Ssd1306 _screen;
        private static BluetoothLEAdvertisementWatcher _watcher;
        private static string SSID = "";
        private static string SSIDPASSWORD = "";
        private static readonly HttpClient HttpClient = new HttpClient();

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            InitializeSSD1306();
            _screen.DrawString(2, 2, "Hello", 2, center: true);
            _screen.Display();
            InitializeWiFi();
            InitializeBluetooth();
            _watcher.Start();
            while (true)
            {
                Thread.Sleep(30000);
                _watcher.Stop();
                _watcher.Start();
            }

            void InitializeSSD1306()
            {
                Configuration.SetPinFunction(17, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);
                _screen = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64);
                _screen.ClearScreen();
                _screen.Font = new BasicFont();
            }

            void InitializeWiFi()
            {
                var cs = new CancellationTokenSource(1200000);
                var success = WifiNetworkHelper.ConnectDhcp(SSID, SSIDPASSWORD, token: cs.Token);
                if (!success)
                {
                    Debug.WriteLine($"Cannot connect to the WiFi, error: {WifiNetworkHelper.Status}");
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                    }
                }
                else
                {
                    Debug.WriteLine("Connected successfully");
                }
            }

            void InitializeBluetooth()
            {
                _watcher = new()
                {
                    ScanningMode = BluetoothLEScanningMode.Active
                };
                _watcher.Received += Watcher_Received;
            }

            static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
            {
                var adv = args.Advertisement;
                foreach (BluetoothLEAdvertisementDataSection d in adv.DataSections)
                {
                    if (d.Data.Length == 26)
                    {
                        var bytes = new byte[d.Data.Length];
                        var dr = DataReader.FromBuffer(d.Data);
                        dr.ReadBytes(bytes);
                        var b = new byte[2];
                        b[0] = bytes[1];
                        b[1] = bytes[0];
                        var tm = BitConverter.ToString(b);
                        if (tm == "04-99")
                        {
                            var message = RuuviDf5Decoder.DecodeMessage(bytes);
                            if (message is not null)
                            {
                                _screen.ClearScreen();
                                _screen.DrawString(2, 2, message.TemperatureDisplay, 1, true);
                                _screen.DrawString(2, 18, message.HumidityDisplay, 1, true);
                                _screen.DrawString(2, 34, message.BatteryDisplay, 1, true);
                                _screen.DrawString(2, 50, message.MacDisplay, 1, true);
                                _screen.Display();
                                Debug.WriteLine($"Temperature: {message.TemperatureDisplay}");
                                Debug.WriteLine($"Humidity: {message.HumidityDisplay}");
                                Debug.WriteLine($"Battery: {message.BatteryDisplay}");
                                Debug.WriteLine($"Mac Address: {message.MacAddress}");
                                Debug.WriteLine($"Time: {message.TimeStamp}");
                                var requestUri = "https://api.thingspeak.com/channels/1417/field/2/last.txt";

                                //var response = HttpClient.Get(requestUri);
                                //response.EnsureSuccessStatusCode();
                                //var responseBody = response.Content.ReadAsString();
                                //Debug.WriteLine($"Response: {responseBody}");
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            Console.WriteLine(tm);
                        }
                    }
                }
            }
        }
    }
}
