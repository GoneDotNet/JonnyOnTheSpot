using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class OpenMeteoService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenMeteoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
    {
        var client = _httpClientFactory.CreateClient("OpenMeteo");

        var forecastUrl = $"/v1/forecast?latitude={latitude}&longitude={longitude}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature," +
            "weather_code,wind_speed_10m,wind_direction_10m,pressure_msl,uv_index,visibility" +
            "&hourly=temperature_2m,precipitation_probability,weather_code,wind_speed_10m" +
            "&daily=temperature_2m_max,temperature_2m_min,weather_code," +
            "precipitation_sum,precipitation_probability_max,sunrise,sunset,uv_index_max,wind_speed_10m_max" +
            "&timezone=auto&forecast_days=7&temperature_unit=fahrenheit&wind_speed_unit=mph";

        var forecastJson = await client.GetStringAsync(forecastUrl);
        var forecastDoc = JsonDocument.Parse(forecastJson);

        var weatherData = new WeatherData();

        // Parse current weather
        var current = forecastDoc.RootElement.GetProperty("current");
        weatherData.Current = new CurrentWeather
        {
            Temperature = current.GetProperty("temperature_2m").GetDouble(),
            ApparentTemperature = current.GetProperty("apparent_temperature").GetDouble(),
            WeatherCode = current.GetProperty("weather_code").GetInt32(),
            WindSpeed = current.GetProperty("wind_speed_10m").GetDouble(),
            WindDirection = current.GetProperty("wind_direction_10m").GetDouble(),
            Humidity = current.GetProperty("relative_humidity_2m").GetDouble(),
            Pressure = current.GetProperty("pressure_msl").GetDouble(),
            UvIndex = current.GetProperty("uv_index").GetDouble(),
            Visibility = current.GetProperty("visibility").GetDouble(),
        };

        // Parse hourly forecasts (next 24 hours)
        var hourly = forecastDoc.RootElement.GetProperty("hourly");
        var hourlyTimes = hourly.GetProperty("time").EnumerateArray().ToList();
        var hourlyTemps = hourly.GetProperty("temperature_2m").EnumerateArray().ToList();
        var hourlyPrecipProb = hourly.GetProperty("precipitation_probability").EnumerateArray().ToList();
        var hourlyCodes = hourly.GetProperty("weather_code").EnumerateArray().ToList();
        var hourlyWind = hourly.GetProperty("wind_speed_10m").EnumerateArray().ToList();

        var now = DateTime.Now;
        for (int i = 0; i < hourlyTimes.Count && weatherData.HourlyForecasts.Count < 24; i++)
        {
            var time = DateTime.Parse(hourlyTimes[i].GetString()!);
            if (time < now) continue;

            weatherData.HourlyForecasts.Add(new HourlyForecast
            {
                Time = time,
                Temperature = hourlyTemps[i].GetDouble(),
                PrecipitationProbability = hourlyPrecipProb[i].ValueKind == JsonValueKind.Null ? 0 : hourlyPrecipProb[i].GetInt32(),
                WeatherCode = hourlyCodes[i].ValueKind == JsonValueKind.Null ? 0 : hourlyCodes[i].GetInt32(),
                WindSpeed = hourlyWind[i].GetDouble(),
            });
        }

        // Parse daily forecasts
        var daily = forecastDoc.RootElement.GetProperty("daily");
        var dailyTimes = daily.GetProperty("time").EnumerateArray().ToList();
        var dailyMaxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().ToList();
        var dailyMinTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().ToList();
        var dailyCodes = daily.GetProperty("weather_code").EnumerateArray().ToList();
        var dailyPrecipSum = daily.GetProperty("precipitation_sum").EnumerateArray().ToList();
        var dailyPrecipProb = daily.GetProperty("precipitation_probability_max").EnumerateArray().ToList();
        var dailySunrise = daily.GetProperty("sunrise").EnumerateArray().ToList();
        var dailySunset = daily.GetProperty("sunset").EnumerateArray().ToList();
        var dailyUv = daily.GetProperty("uv_index_max").EnumerateArray().ToList();
        var dailyWind = daily.GetProperty("wind_speed_10m_max").EnumerateArray().ToList();

        for (int i = 0; i < dailyTimes.Count; i++)
        {
            weatherData.DailyForecasts.Add(new DailyForecast
            {
                Date = DateTime.Parse(dailyTimes[i].GetString()!),
                TemperatureMax = dailyMaxTemps[i].GetDouble(),
                TemperatureMin = dailyMinTemps[i].GetDouble(),
                WeatherCode = dailyCodes[i].ValueKind == JsonValueKind.Null ? 0 : dailyCodes[i].GetInt32(),
                PrecipitationSum = dailyPrecipSum[i].GetDouble(),
                PrecipitationProbabilityMax = dailyPrecipProb[i].ValueKind == JsonValueKind.Null ? 0 : dailyPrecipProb[i].GetInt32(),
                Sunrise = DateTime.Parse(dailySunrise[i].GetString()!),
                Sunset = DateTime.Parse(dailySunset[i].GetString()!),
                UvIndexMax = dailyUv[i].GetDouble(),
                WindSpeedMax = dailyWind[i].GetDouble(),
            });
        }

        // Fetch air quality
        try
        {
            var aqClient = _httpClientFactory.CreateClient("OpenMeteoAirQuality");
            var aqUrl = $"/v1/air-quality?latitude={latitude}&longitude={longitude}&hourly=us_aqi,pm10,pm2_5";
            var aqJson = await aqClient.GetStringAsync(aqUrl);
            var aqDoc = JsonDocument.Parse(aqJson);
            var aqHourly = aqDoc.RootElement.GetProperty("hourly");
            var aqTimes = aqHourly.GetProperty("time").EnumerateArray().ToList();
            var aqAqi = aqHourly.GetProperty("us_aqi").EnumerateArray().ToList();
            var aqPm10 = aqHourly.GetProperty("pm10").EnumerateArray().ToList();
            var aqPm25 = aqHourly.GetProperty("pm2_5").EnumerateArray().ToList();

            // Find the closest hour to now
            var nowHour = now.ToString("yyyy-MM-ddTHH:00");
            for (int i = 0; i < aqTimes.Count; i++)
            {
                if (aqTimes[i].GetString() == nowHour)
                {
                    weatherData.AirQuality = new AirQuality
                    {
                        UsAqi = aqAqi[i].ValueKind == JsonValueKind.Null ? 0 : aqAqi[i].GetInt32(),
                        Pm10 = aqPm10[i].ValueKind == JsonValueKind.Null ? 0 : aqPm10[i].GetDouble(),
                        Pm25 = aqPm25[i].ValueKind == JsonValueKind.Null ? 0 : aqPm25[i].GetDouble(),
                    };
                    break;
                }
            }
        }
        catch
        {
            // Air quality is optional, don't fail the whole request
        }

        return weatherData;
    }

    public async Task<LocationWeatherSummary> GetWeatherSummaryAsync(SavedLocation location)
    {
        var client = _httpClientFactory.CreateClient("OpenMeteo");
        var url = $"/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}" +
            "&current=temperature_2m,weather_code" +
            "&daily=temperature_2m_max,temperature_2m_min" +
            "&timezone=auto&forecast_days=1&temperature_unit=fahrenheit";

        var json = await client.GetStringAsync(url);
        var doc = JsonDocument.Parse(json);

        var current = doc.RootElement.GetProperty("current");
        var daily = doc.RootElement.GetProperty("daily");

        return new LocationWeatherSummary
        {
            Location = location,
            Temperature = current.GetProperty("temperature_2m").GetDouble(),
            WeatherCode = current.GetProperty("weather_code").GetInt32(),
            TemperatureHigh = daily.GetProperty("temperature_2m_max").EnumerateArray().First().GetDouble(),
            TemperatureLow = daily.GetProperty("temperature_2m_min").EnumerateArray().First().GetDouble(),
        };
    }
}
