using System;

namespace NanoTemp.Data
{
    public class RuuviDf5Decoder
    {
        public static Measurement DecodeMessage(byte[] message)
        {
            if (message.Length != 26)
            {
                Console.WriteLine("Invalid message format");
                return null;
                //throw new ArgumentException("Message too short");
            }
            var header = message[0];
            var format = message[2];
            var payload = new byte[message.Length - 3];
            Array.Copy(message, 3, payload, 0, payload.Length);
            if (format != 5)
            {
                Console.WriteLine($"Wrong message format, expected 5 but was {format}");
                return null;
            }

            Console.WriteLine("Creating a Measurement");
            return new Measurement
            {
                Temperature = GetTemp(payload[0], payload[1]),
                Humidity = GetHumidity(payload[2], payload[3]),
                MacAddress = GetMac(payload),
                Battery = GetBattery(payload[12], payload[13]),
                TimeStamp = DateTime.UtcNow
            };
        }
        private static string GetMac(byte[] payload)
        {
            var mac = new byte[payload.Length - 17];
            Array.Copy(payload, 17, mac, 0, mac.Length);
            return BitConverter.ToString(payload);
        }
        private static double GetTemp(int p1, int p2)
        {
            var value = TwosComplement((p1 << 8) + p2) / 200;
            return value;
        }
        private static double GetHumidity(int p1, int p2)
        {
            if (p1 == 255 && p2 == 255)
            {
                return 0;
            }
            var value = ((double)((p1 & 255) << 8 | p2 & 255)) / 400;
            return value;
        }
        private static double TwosComplement(int value)
        {
            if ((value & (1 << (16 - 1))) != 0)
            {
                value = value - (1 << 16);
            }
            return (double)value;
        }
        private static double GetBattery(int p1, int p2)
        {
            var power = (p1 & 255) << 8 | (p2 & 255);
            var voltage = ((power % 4294967296) >> 5) + 1600;
            if (((power % 4294967296) >> 5) == 194686858891537)
            {
                return 0;
            }
            return ((double)voltage) / 1000;
        }
    }
}
