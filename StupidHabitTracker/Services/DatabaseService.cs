using SQLite;
using StupidHabitTracker.Models;

namespace StupidHabitTracker.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection _db = null!;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "habittracker.db3");
    }

    public async Task InitializeAsync()
    {
        if (_db is not null) return;
        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Habit>();
        await _db.CreateTableAsync<TimedSession>();
    }

    public async Task<List<Habit>> GetHabitsAsync()
    {
        await InitializeAsync();
        return await _db.Table<Habit>().OrderByDescending(h => h.CreatedAt).ToListAsync();
    }

    public async Task<Habit> GetOrCreateHabitAsync(string name)
    {
        await InitializeAsync();
        var existing = await _db.Table<Habit>().FirstOrDefaultAsync(h => h.Name == name);
        if (existing is not null) return existing;

        var habit = new Habit { Name = name, CreatedAt = DateTime.UtcNow };
        await _db.InsertAsync(habit);
        return habit;
    }

    public async Task<List<TimedSession>> GetSessionsAsync(int limit = 50)
    {
        await InitializeAsync();
        return await _db.Table<TimedSession>()
            .OrderByDescending(s => s.StartTime)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TimedSession>> GetSessionsForDateRangeAsync(DateTime start, DateTime end)
    {
        await InitializeAsync();
        return await _db.Table<TimedSession>()
            .Where(s => s.StartTime >= start && s.StartTime <= end)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task SaveSessionAsync(TimedSession session)
    {
        await InitializeAsync();
        if (session.Id == 0)
            await _db.InsertAsync(session);
        else
            await _db.UpdateAsync(session);
    }

    public async Task DeleteSessionAsync(TimedSession session)
    {
        await InitializeAsync();
        await _db.DeleteAsync(session);
    }

    public async Task<int> GetTotalSessionCountAsync()
    {
        await InitializeAsync();
        return await _db.Table<TimedSession>().CountAsync();
    }

    public async Task<double> GetTotalTimeSecondsAsync()
    {
        await InitializeAsync();
        var sessions = await _db.Table<TimedSession>().ToListAsync();
        return sessions.Sum(s => s.DurationSeconds);
    }

    public async Task<List<string>> GetRecentHabitNamesAsync(int count = 5)
    {
        await InitializeAsync();
        var sessions = await _db.Table<TimedSession>()
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
        return sessions
            .Select(s => s.HabitName)
            .Distinct()
            .Take(count)
            .ToList();
    }
}
