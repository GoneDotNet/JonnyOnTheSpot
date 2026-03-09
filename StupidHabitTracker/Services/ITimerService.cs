namespace StupidHabitTracker.Services;

public interface ITimerService
{
    bool IsRunning { get; }
    bool IsPaused { get; }
    TimeSpan Elapsed { get; }
    DateTime? StartTime { get; }
    
    event Action<TimeSpan>? Ticked;
    event Action? Started;
    event Action? Stopped;
    event Action? Paused;
    event Action? Resumed;
    
    void Start();
    void Stop();
    void Pause();
    void Resume();
    void Reset();
}
