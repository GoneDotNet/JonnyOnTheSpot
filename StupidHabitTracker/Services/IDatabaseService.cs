using StupidHabitTracker.Models;

namespace StupidHabitTracker.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Habit>> GetHabitsAsync();
    Task<Habit> GetOrCreateHabitAsync(string name);
    Task<List<TimedSession>> GetSessionsAsync(int limit = 50);
    Task<List<TimedSession>> GetSessionsForDateRangeAsync(DateTime start, DateTime end);
    Task SaveSessionAsync(TimedSession session);
    Task DeleteSessionAsync(TimedSession session);
    Task<int> GetTotalSessionCountAsync();
    Task<double> GetTotalTimeSecondsAsync();
    Task<List<string>> GetRecentHabitNamesAsync(int count = 5);
}
