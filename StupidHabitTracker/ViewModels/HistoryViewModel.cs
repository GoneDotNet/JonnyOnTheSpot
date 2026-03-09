using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StupidHabitTracker.Models;
using StupidHabitTracker.Services;

namespace StupidHabitTracker.ViewModels;

public class SessionGroup : ObservableCollection<TimedSession>
{
    public string DateHeader { get; }
    public DateTime Date { get; }

    public SessionGroup(DateTime date, IEnumerable<TimedSession> sessions) : base(sessions)
    {
        Date = date;
        DateHeader = date.Date == DateTime.Today ? "Today"
            : date.Date == DateTime.Today.AddDays(-1) ? "Yesterday"
            : date.ToString("dddd, MMM d");
    }
}

public partial class HistoryViewModel : ObservableObject
{
    private readonly IDatabaseService _db;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<SessionGroup> SessionGroups { get; } = new();

    public HistoryViewModel(IDatabaseService db)
    {
        _db = db;
    }

    [RelayCommand]
    private async Task LoadSessionsAsync()
    {
        IsLoading = true;
        try
        {
            var sessions = await _db.GetSessionsAsync(100);
            SessionGroups.Clear();

            var groups = sessions
                .GroupBy(s => s.StartTime.ToLocalTime().Date)
                .OrderByDescending(g => g.Key);

            foreach (var group in groups)
            {
                SessionGroups.Add(new SessionGroup(group.Key, group.OrderByDescending(s => s.StartTime)));
            }

            IsEmpty = SessionGroups.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(TimedSession session)
    {
        await _db.DeleteSessionAsync(session);
        await LoadSessionsAsync();
    }

    public static string FormatDuration(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds:D2}s";
        return $"{ts.Seconds}s";
    }

    public static string FormatTime(DateTime utcTime)
    {
        return utcTime.ToLocalTime().ToString("h:mm tt");
    }
}
