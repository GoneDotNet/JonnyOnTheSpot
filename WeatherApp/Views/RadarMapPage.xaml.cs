using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class RadarMapPage : ContentPage
{
    private readonly RadarMapViewModel _viewModel;

    public RadarMapPage(RadarMapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(RadarMapViewModel.RadarFramesJson) && _viewModel.Location != null)
            {
                LoadRadarMap();
            }
        };
    }

    private void LoadRadarMap()
    {
        var lat = _viewModel.Location!.Latitude;
        var lon = _viewModel.Location!.Longitude;
        var name = _viewModel.Location!.Name;
        var framesJson = _viewModel.RadarFramesJson;

        var html = GenerateRadarHtml(lat, lon, name, framesJson);
        RadarWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string GenerateRadarHtml(double lat, double lon, string locationName, string framesJson)
    {
        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
            <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
            <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                html, body { width: 100%; height: 100%; overflow: hidden; }
                #map { width: 100%; height: 100%; }
                .controls {
                    position: absolute; bottom: 24px; left: 50%; transform: translateX(-50%);
                    z-index: 1000; background: rgba(15, 22, 36, 0.9); border-radius: 16px;
                    padding: 12px 20px; display: flex; align-items: center; gap: 12px;
                    backdrop-filter: blur(10px); border: 1px solid rgba(255,255,255,0.1);
                }
                .controls button {
                    background: #2A4060; border: none; color: white; width: 36px; height: 36px;
                    border-radius: 50%; font-size: 16px; cursor: pointer;
                }
                .controls button:active { background: #3A5070; }
                #timeline { flex: 1; min-width: 120px; accent-color: #5B9BD5; }
                #timestamp { color: #8899AA; font-size: 12px; font-family: -apple-system, sans-serif; min-width: 60px; text-align: center; }
                .leaflet-tile-pane { opacity: 1; }
            </style>
        </head>
        <body>
            <div id="map"></div>
            <div class="controls">
                <button id="playPause" onclick="togglePlay()">▶</button>
                <input type="range" id="timeline" min="0" max="0" value="0" oninput="setFrame(this.value)" />
                <span id="timestamp">--:--</span>
            </div>
            <script>
                const map = L.map('map', {
                    center: [{{lat}}, {{lon}}],
                    zoom: 7,
                    zoomControl: false
                });

                L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
                    attribution: '&copy; OSM &copy; CARTO',
                    subdomains: 'abcd', maxZoom: 19
                }).addTo(map);

                L.marker([{{lat}}, {{lon}}]).addTo(map)
                    .bindPopup('{{locationName}}').openPopup();

                const frames = {{framesJson}};
                let currentLayer = null;
                let currentFrame = 0;
                let playing = false;
                let interval = null;

                const timeline = document.getElementById('timeline');
                const timestampEl = document.getElementById('timestamp');
                const playBtn = document.getElementById('playPause');

                if (frames.length > 0) {
                    timeline.max = frames.length - 1;
                    timeline.value = frames.length - 1;
                    setFrame(frames.length - 1);
                }

                function setFrame(idx) {
                    idx = parseInt(idx);
                    if (idx < 0 || idx >= frames.length) return;
                    currentFrame = idx;
                    timeline.value = idx;

                    if (currentLayer) map.removeLayer(currentLayer);
                    currentLayer = L.tileLayer(frames[idx].tileUrl, {
                        opacity: 0.6, zIndex: 10
                    }).addTo(map);

                    const d = new Date(frames[idx].time * 1000);
                    timestampEl.textContent = d.toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'});
                }

                function togglePlay() {
                    playing = !playing;
                    playBtn.textContent = playing ? '⏸' : '▶';
                    if (playing) {
                        currentFrame = 0;
                        setFrame(0);
                        interval = setInterval(() => {
                            currentFrame++;
                            if (currentFrame >= frames.length) { currentFrame = 0; }
                            setFrame(currentFrame);
                        }, 500);
                    } else {
                        clearInterval(interval);
                    }
                }
            </script>
        </body>
        </html>
        """;
    }
}
