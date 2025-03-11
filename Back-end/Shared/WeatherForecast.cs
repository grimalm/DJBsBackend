namespace Back_end.Shared
{
    public class WeatherApiData
    {
        public WeatherData? current {get; set;}

    }
    
    public class WeatherData
    {
        public DateTime time { get; set; }
        public double temperature_2m { get; set; }
        public double relative_humidity_2m { get; set; }
        public double rain { get; set; }
        public double surface_pressure { get; set; }
        public double wind_speed_10m { get; set; }

    }
}
