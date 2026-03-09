namespace StupidHabitTracker.Services;

public class TimerService : ITimerService, IDisposable
{
    private System.Timers.Timer? _timer;
    private DateTime _startTime;
    private TimeSpan _pausedElapsed;
    
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public TimeSpan Elapsed => IsRunning && !IsPaused 
        ? DateTime.UtcNow - _startTime + _pausedElapsed
        : _pausedElapsed;
    public DateTime? StartTime => IsRunning ? _startTime - _pausedElapsed : null;
    
    public event Action<TimeSpan>? Ticked;
    public event Action? Started;
    public event Action? Stopped;
    public event Action? Paused;
    public event Action? Resumed;

    public void Start()
    {
        _startTime = DateTime.UtcNow;
        _pausedElapsed = TimeSpan.Zero;
        IsRunning = true;
        IsPaused = false;
        
        _timer = new System.Timers.Timer(100); // tick every 100ms for smooth display
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
        
        Started?.Invoke();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        
        _pausedElapsed = Elapsed;
        IsRunning = false;
        IsPaused = false;
        
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        
        Stopped?.Invoke();
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused) return;
        
        _pausedElapsed = Elapsed;
        IsPaused = true;
        
        _timer?.Stop();
        
        Paused?.Invoke();
    }

    public void Resume()
    {
        if (!IsRunning || !IsPaused) return;
        
        _startTime = DateTime.UtcNow;
        IsPaused = false;
        
        _timer?.Start();
        
        Resumed?.Invoke();
    }

    public void Reset()
    {
        Stop();
        _pausedElapsed = TimeSpan.Zero;
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Ticked?.Invoke(Elapsed);
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
