using System.Text.Json.Serialization;
using GeoSnappy.Models;

namespace GeoSnappy;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Photo))]
[JsonSerializable(typeof(List<Photo>))]
public partial class AppJsonContext : JsonSerializerContext;
