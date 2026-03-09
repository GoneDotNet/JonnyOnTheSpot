namespace WeatherApp.Models;

public class WeatherData
{
    public CurrentWeather Current { get; set; } = new();
    public List<HourlyForecast> HourlyForecasts { get; set; } = [];
    public List<DailyForecast> DailyForecasts { get; set; } = [];
    public AirQuality? AirQuality { get; set; }
}

public class CurrentWeather
{
    public double Temperature { get; set; }
    public double ApparentTemperature { get; set; }
    public int WeatherCode { get; set; }
    public double WindSpeed { get; set; }
    public double WindDirection { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public double UvIndex { get; set; }
    public double Visibility { get; set; }
    public string VisibilityDisplay
    {
        get
        {
            var miles = Visibility / 1609.34;
            return miles >= 10 ? $"{miles:F0} mi" : $"{miles:F1} mi";
        }
    }
    public string Description => WmoWeatherCode.GetDescription(WeatherCode);
    public string Icon => WmoWeatherCode.GetIcon(WeatherCode);
}

public class HourlyForecast
{
    public DateTime Time { get; set; }
    public double Temperature { get; set; }
    public int PrecipitationProbability { get; set; }
    public int WeatherCode { get; set; }
    public double WindSpeed { get; set; }
    public string Icon => WmoWeatherCode.GetIcon(WeatherCode);
    public string TimeLabel => Time.ToString("ha").ToLower();
}

public class DailyForecast
{
    public DateTime Date { get; set; }
    public double TemperatureMax { get; set; }
    public double TemperatureMin { get; set; }
    public int WeatherCode { get; set; }
    public double PrecipitationSum { get; set; }
    public int PrecipitationProbabilityMax { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public double UvIndexMax { get; set; }
    public double WindSpeedMax { get; set; }
    public string Icon => WmoWeatherCode.GetIcon(WeatherCode);
    public string DayLabel => Date.Date == DateTime.Today ? "Today" : Date.ToString("ddd");
}

public class AirQuality
{
    public int UsAqi { get; set; }
    public double Pm10 { get; set; }
    public double Pm25 { get; set; }
    public string Level => UsAqi switch
    {
        <= 50 => "Good",
        <= 100 => "Moderate",
        <= 150 => "Unhealthy for Sensitive",
        <= 200 => "Unhealthy",
        <= 300 => "Very Unhealthy",
        _ => "Hazardous"
    };
    public Color LevelColor => UsAqi switch
    {
        <= 50 => Colors.Green,
        <= 100 => Colors.Yellow,
        <= 150 => Colors.Orange,
        <= 200 => Colors.Red,
        <= 300 => Colors.Purple,
        _ => Colors.Maroon
    };
}

public class LocationWeatherSummary
{
    public SavedLocation Location { get; set; } = new();
    public double Temperature { get; set; }
    public double TemperatureHigh { get; set; }
    public double TemperatureLow { get; set; }
    public int WeatherCode { get; set; }
    public string Description => WmoWeatherCode.GetDescription(WeatherCode);
    public string Icon => WmoWeatherCode.GetIcon(WeatherCode);
}
