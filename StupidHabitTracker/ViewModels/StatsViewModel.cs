using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StupidHabitTracker.Services;

namespace StupidHabitTracker.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    private readonly IDatabaseService _db;

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private string _totalTimeDisplay = "0m";

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _bestStreak;

    [ObservableProperty]
    private string _averageSessionDisplay = "0m";

    [ObservableProperty]
    private bool _hasData;

    public ObservableCollection<DayStats> WeeklyData { get; } = new();
    public ObservableCollection<HabitStat> TopHabits { get; } = new();

    public StatsViewModel(IDatabaseService db)
    {
        _db = db;
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        TotalSessions = await _db.GetTotalSessionCountAsync();
        var totalSeconds = await _db.GetTotalTimeSecondsAsync();
        TotalTimeDisplay = FormatTotalTime(totalSeconds);

        if (TotalSessions > 0)
        {
            AverageSessionDisplay = FormatTotalTime(totalSeconds / TotalSessions);
        }

        // Calculate streaks
        await CalculateStreaksAsync();

        // Weekly data
        await LoadWeeklyDataAsync();

        // Top habits
        await LoadTopHabitsAsync();

        HasData = TotalSessions > 0;
    }

    private async Task CalculateStreaksAsync()
    {
        var sessions = await _db.GetSessionsAsync(1000);
        if (sessions.Count == 0)
        {
            CurrentStreak = 0;
            BestStreak = 0;
            return;
        }

        var daysWithSessions = sessions
            .Select(s => s.StartTime.ToLocalTime().Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        // Current streak
        int current = 0;
        var checkDate = DateTime.Today;
        foreach (var day in daysWithSessions)
        {
            if (day == checkDate)
            {
                current++;
                checkDate = checkDate.AddDays(-1);
            }
            else if (day == checkDate.AddDays(-1) && current == 0)
            {
                // Allow starting from yesterday
                checkDate = day;
                current++;
                checkDate = checkDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }
        CurrentStreak = current;

        // Best streak
        int best = 0;
        int streak = 1;
        for (int i = 1; i < daysWithSessions.Count; i++)
        {
            if ((daysWithSessions[i - 1] - daysWithSessions[i]).Days == 1)
            {
                streak++;
            }
            else
            {
                best = Math.Max(best, streak);
                streak = 1;
            }
        }
        best = Math.Max(best, streak);
        BestStreak = best;
    }

    private async Task LoadWeeklyDataAsync()
    {
        WeeklyData.Clear();
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // Monday
        if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            startOfWeek = startOfWeek.AddDays(-7);

        var sessions = await _db.GetSessionsForDateRangeAsync(
            startOfWeek.ToUniversalTime(),
            startOfWeek.AddDays(7).ToUniversalTime());

        var maxMinutes = 1.0; // avoid division by zero

        var dailyTotals = new List<(string Label, double Minutes)>();
        for (int i = 0; i < 7; i++)
        {
            var day = startOfWeek.AddDays(i);
            var dayLabel = day.ToString("ddd");
            var dayMinutes = sessions
                .Where(s => s.StartTime.ToLocalTime().Date == day)
                .Sum(s => s.DurationSeconds) / 60.0;
            dailyTotals.Add((dayLabel, dayMinutes));
            maxMinutes = Math.Max(maxMinutes, dayMinutes);
        }

        foreach (var (label, minutes) in dailyTotals)
        {
            WeeklyData.Add(new DayStats
            {
                DayLabel = label,
                Minutes = minutes,
                BarHeight = Math.Max(4, (minutes / maxMinutes) * 120),
                IsToday = label == DateTime.Today.ToString("ddd"),
                DisplayValue = minutes >= 60
                    ? $"{minutes / 60:F0}h"
                    : minutes >= 1
                        ? $"{minutes:F0}m"
                        : ""
            });
        }
    }

    private async Task LoadTopHabitsAsync()
    {
        TopHabits.Clear();
        var sessions = await _db.GetSessionsAsync(500);
        var habitGroups = sessions
            .GroupBy(s => s.HabitName)
            .Select(g => new HabitStat
            {
                Name = g.Key,
                SessionCount = g.Count(),
                TotalSeconds = g.Sum(s => s.DurationSeconds),
                TotalDisplay = FormatTotalTime(g.Sum(s => s.DurationSeconds))
            })
            .OrderByDescending(h => h.TotalSeconds)
            .Take(5);

        foreach (var habit in habitGroups)
            TopHabits.Add(habit);
    }

    private static string FormatTotalTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}m";
        return $"{ts.Seconds}s";
    }
}

public class DayStats
{
    public string DayLabel { get; set; } = "";
    public double Minutes { get; set; }
    public double BarHeight { get; set; }
    public bool IsToday { get; set; }
    public string DisplayValue { get; set; } = "";
}

public class HabitStat
{
    public string Name { get; set; } = "";
    public int SessionCount { get; set; }
    public double TotalSeconds { get; set; }
    public string TotalDisplay { get; set; } = "";
}
