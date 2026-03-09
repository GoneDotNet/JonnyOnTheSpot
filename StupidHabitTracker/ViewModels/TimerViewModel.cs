using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StupidHabitTracker.Models;
using StupidHabitTracker.Services;

namespace StupidHabitTracker.ViewModels;

public partial class TimerViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly ITimerService _timer;

    [ObservableProperty]
    private string _habitName = string.Empty;

    [ObservableProperty]
    private string _elapsedDisplay = "00:00";

    [ObservableProperty]
    private double _elapsedSeconds;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _hasHabitName;

    [ObservableProperty]
    private string _actionButtonText = "Start";

    [ObservableProperty]
    private string _lastSessionSummary = string.Empty;

    [ObservableProperty]
    private bool _showLastSession;

    public ObservableCollection<string> RecentHabits { get; } = new();

    public TimerViewModel(IDatabaseService db, ITimerService timer)
    {
        _db = db;
        _timer = timer;

        _timer.Ticked += OnTimerTicked;
        _timer.Started += () => MainThread.BeginInvokeOnMainThread(() =>
        {
            IsRunning = true;
            IsPaused = false;
            ActionButtonText = "Stop";
        });
        _timer.Stopped += () => MainThread.BeginInvokeOnMainThread(() =>
        {
            IsRunning = false;
            IsPaused = false;
            ActionButtonText = "Start";
        });
    }

    private void OnTimerTicked(TimeSpan elapsed)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ElapsedSeconds = elapsed.TotalSeconds;
            ElapsedDisplay = elapsed.TotalHours >= 1
                ? elapsed.ToString(@"h\:mm\:ss")
                : elapsed.ToString(@"mm\:ss");
        });
    }

    partial void OnHabitNameChanged(string value)
    {
        HasHabitName = !string.IsNullOrWhiteSpace(value);
    }

    [RelayCommand]
    private async Task ToggleTimerAsync()
    {
        if (!IsRunning)
        {
            if (string.IsNullOrWhiteSpace(HabitName)) return;
            ShowLastSession = false;
            _timer.Start();
        }
        else
        {
            _timer.Stop();
            await SaveSessionAsync();
        }
    }

    [RelayCommand]
    private void PauseResume()
    {
        if (IsPaused)
        {
            _timer.Resume();
            IsPaused = false;
            ActionButtonText = "Stop";
        }
        else
        {
            _timer.Pause();
            IsPaused = true;
            ActionButtonText = "Resume";
        }
    }

    [RelayCommand]
    private void SelectHabit(string habitName)
    {
        HabitName = habitName;
    }

    [RelayCommand]
    private async Task LoadRecentHabitsAsync()
    {
        var recent = await _db.GetRecentHabitNamesAsync(6);
        RecentHabits.Clear();
        foreach (var name in recent)
            RecentHabits.Add(name);
    }

    private async Task SaveSessionAsync()
    {
        if (string.IsNullOrWhiteSpace(HabitName)) return;

        var habit = await _db.GetOrCreateHabitAsync(HabitName.Trim());
        var startTime = _timer.StartTime ?? DateTime.UtcNow.AddSeconds(-ElapsedSeconds);
        var session = new TimedSession
        {
            HabitId = habit.Id,
            HabitName = habit.Name,
            StartTime = startTime,
            EndTime = DateTime.UtcNow,
            DurationSeconds = ElapsedSeconds
        };

        await _db.SaveSessionAsync(session);

        // Show summary
        var duration = TimeSpan.FromSeconds(ElapsedSeconds);
        LastSessionSummary = $"✓ {habit.Name} — {FormatDuration(duration)}";
        ShowLastSession = true;

        // Reset
        ElapsedDisplay = "00:00";
        ElapsedSeconds = 0;
        HabitName = string.Empty;

        // Refresh recent habits
        await LoadRecentHabitsAsync();
    }

    private static string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds:D2}s";
        return $"{ts.Seconds}s";
    }
}
