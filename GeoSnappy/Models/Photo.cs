using System.Text.Json.Serialization;

namespace GeoSnappy.Models;

public class Photo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    /// <summary>Relative filename only (e.g. "geosnappy_20260306_200433.jpg")</summary>
    public string FilePath { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CapturedAt { get; set; }

    /// <summary>Full path resolved at runtime — survives app container changes on redeploy.</summary>
    [JsonIgnore]
    public string FullFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(FilePath)) return string.Empty;
            // Handle legacy absolute paths by extracting just the filename
            var fileName = Path.IsPathRooted(FilePath) ? Path.GetFileName(FilePath) : FilePath;
            return Path.Combine(FileSystem.AppDataDirectory, fileName);
        }
    }
}
