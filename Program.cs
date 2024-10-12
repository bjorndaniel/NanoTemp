using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Networking;
using NanoTemp.Data;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace NanoTemp
{
    public class Program
    {
        private static BluetoothLEAdvertisementWatcher _watcher;
        const string ssid = "SSID";
        const string password = "PWD";
        private static HttpClient _httpClient;
        private static Measurement _message;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            InitializeWifi();
            InitializeBluetooth();
            _watcher.Start();
            while (true)
            {
                Thread.Sleep(30000);
                _watcher.Stop();
                _watcher.Start();
            }

            void InitializeBluetooth()
            {
                _watcher = new();
                _watcher.Received += Watcher_Received;
            }

            void InitializeWifi()
            {
                var cs = new CancellationTokenSource(20_000);
                var success = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token);
                if (!success)
                {
                    Debug.WriteLine($"Cannot connect to the WiFi");
                    //if (WifiNetworkHelper.HelperException != null)
                    //{
                    //    Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                    //}
                }
                else
                {
                    Debug.WriteLine("Connected successfully");
                    _httpClient = new HttpClient();
                }
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
                        if (tm == "04-99") //https://www.bluetooth.com/wp-content/uploads/Files/Specification/HTML/Assigned_Numbers/out/en/Assigned_Numbers.pdf?v=1728219678943
                        {
                            _message = RuuviDf5Decoder.DecodeMessage(bytes);
                            if (_message is not null)
                            {
                                //_httpClient ??= new HttpClient();
                                //var requestUri = "http://5058/measure";
                                //var content = new StringContent(JsonConvert.SerializeObject(message));
                                //var response = _httpClient.Post(requestUri, content);
                                //var mess = response.Content.ReadAsString();
                                //Debug.WriteLine(mess);
                                Debug.WriteLine($"Temperature: {_message.TemperatureDisplay}");
                                Debug.WriteLine($"Humidity: {_message.HumidityDisplay}");
                                Debug.WriteLine($"Battery: {_message.BatteryDisplay}");
                                Debug.WriteLine($"Mac Address: {_message.MacAddress}");
                                Debug.WriteLine($"Time: {_message.TimeStamp}");
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            Debug.WriteLine(tm);
                        }
                    }
                }
            }
        }
    }
}