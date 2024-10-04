using Iot.Device.Ssd13xx;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Hardware.Esp32;
using NanoTemp.Data;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace NanoTemp
{
    public class Program
    {
        private static Ssd1306 _screen;
        private static BluetoothLEAdvertisementWatcher _watcher;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            InitializeSSD1306();
            InitializeBluetooth();
            _screen.DrawString(2, 2, "Hello", 2, center: true);
            _screen.Display();
            _watcher.Start();
            Thread.Sleep(Timeout.Infinite);

            void InitializeSSD1306()
            {
                Configuration.SetPinFunction(17, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);
                _screen = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64);
                _screen.ClearScreen();
                _screen.Font = new BasicFont();
            }

            void InitializeBluetooth()
            {
                _watcher = new()
                {
                    // Use active scans to get extra information from devices, scan responses.
                    ScanningMode = BluetoothLEScanningMode.Active
                };
                _watcher.Received += Watcher_Received;
            }

            static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
            {
                //Console.WriteLine("Advertisment received");
                var adv = args.Advertisement;
                foreach (BluetoothLEAdvertisementDataSection d in adv.DataSections)
                {
                    if (d.Data.Length == 26)
                    {
                        //Console.WriteLine($"Data Section Type:{d.DataType} Length:{d.Data.Length}");
                        var bytes = new byte[d.Data.Length];
                        var dr = DataReader.FromBuffer(d.Data);
                        dr.ReadBytes(bytes);
                        var b = new byte[2];
                        b[0] = bytes[0];
                        b[1] = bytes[1];
                        var tm = BitConverter.ToString(b);
                        if (tm == "99-04")
                        {
                            var message = RuuviDf5Decoder.DecodeMessage(bytes);
                            if (message is not null)
                            {
                                _screen.ClearScreen();
                                _screen.DrawString(2, 2, message.TemperatureDisplay, 1, true);//centered text
                                _screen.DrawString(2, 18, message.HumidityDisplay, 1, true);//centered text
                                _screen.Display();
                                Console.WriteLine($"Temperature: {message.TemperatureDisplay}");
                                Console.WriteLine($"Humidity: {message.HumidityDisplay}");
                                Console.WriteLine($"Battery: {message.BatteryDisplay}");
                                Console.WriteLine($"Mac Address: {message.MacAddress}");
                                Console.WriteLine($"Time: {message.TimeStamp}");
                            }
                        }

                    }
                }

                //Console.WriteLine();
                //Console.WriteLine($"=== Advert received ==== {DateTime.Today}");
                //Console.WriteLine($"Address:{args.BluetoothAddress:X} RSSI:{args.RawSignalStrengthInDBm}");
                //Console.WriteLine($"Local name:{adv.LocalName}");
                //Console.WriteLine($"Data Sections:{adv.DataSections.Count}");
                //Console.WriteLine($"Flags: {adv.Flags}");
                //foreach(BluetoothLEAdvertisementDataSection d in adv.DataSections)
                //{
                //    byte[] bytes = new byte[d.Data.Length];
                //    var dr = DataReader.FromBuffer(d.Data);
                //    dr.ReadBytes(bytes);    
                //    var message = RuuviDf5Decoder.DecodeMessage(bytes);
                //    if (message is not null)
                //    {
                //        Console.WriteLine($"Temperature: {message.TemperatureDisplay}");
                //        Console.WriteLine($"Humidity: {message.HumidityDisplay}");
                //        Console.WriteLine($"Battery: {message.BatteryDisplay}");
                //        Console.WriteLine($"Mac Address: {message.MacAddress}");
                //    }
                //}

                //adv.DataSections.ForEach(ds =>
                //{
                //    Console.WriteLine($"Data Section Type:{ds.DataType} Length:{ds.Data.Length}");
                //});
                // List Manufacturers data
                //Console.WriteLine($"Manufacturers Data:{adv.ManufacturerData.Count}");
                //foreach (BluetoothLEManufacturerData md in adv.ManufacturerData)
                //{
                //    Console.WriteLine($"-- Company:{md.CompanyId} Length:{md.Data.Length}");

                //    DataReader dr = DataReader.FromBuffer(md.Data);
                //    byte[] bytes = new byte[md.Data.Length];
                //    dr.ReadBytes(bytes);
                //    //Console.WriteLine($"Data:{BitConverter.ToString(bytes)}");
                //    //var message = RuuviDf5Decoder.DecodeMessage(bytes);
                //    //if (message is not null)
                //    //{
                //    //    Console.WriteLine($"Temperature: {message.TemperatureDisplay}");
                //    //    Console.WriteLine($"Humidity: {message.HumidityDisplay}");
                //    //    Console.WriteLine($"Battery: {message.BatteryDisplay}");
                //    //    Console.WriteLine($"Mac Address: {message.MacAddress}");

                //    //    //foreach (byte b in bytes)
                //    //    //{
                //    //    //    Console.Write($"{b:X}");
                //    //    //}
                //    //    //Console.WriteLine();
                //    //}
                //}

                // List Service UUIDS in Advertisement
                //Console.WriteLine($"Service UUIDS:{adv.ServiceUuids.Length}");

                // There is limited space in adverts you may not get any service UUIDs
                // Maybe just the primary service.
                //foreach (Guid uuid in adv.ServiceUuids)
                //{
                //    Console.WriteLine($" - Advertised service:{uuid}");
                //}
            }
        }
    }
}
