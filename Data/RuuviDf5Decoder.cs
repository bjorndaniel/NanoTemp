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
            }
            var header = message[0];
            var format = message[2];    
            var payload = new SpanByte(message, 3, message.Length - 3);
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
                TimeStamp = DateTime.UtcNow//TODO: Get internet time
            };
        }

        private static string GetMac(SpanByte payload) =>
            BitConverter.ToString(payload.Slice(18).ToArray());

        private static double GetTemp(int p1, int p2) => 
            TwosComplement((p1 << 8) + p2) / 200;

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
