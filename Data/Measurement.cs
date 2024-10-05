using System;

namespace NanoTemp.Data
{
    public class Measurement
    {
        public Guid MeasurementId { get; set; }
        public Guid TripId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public string MacAddress { get; set; } = "";
        public string TagName { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Battery { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TemperatureDisplay => $"Temp: {Temperature}°C";
        public string HumidityDisplay => $"Humid: {Humidity}%";
        public string LatitudeDisplay => $"Lat: {Latitude}";
        public string LongitudeDisplay => $"Long: {Longitude}";
        public string BatteryDisplay => $"Battery: {Battery}V";
    }
}
