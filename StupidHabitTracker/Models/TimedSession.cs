using SQLite;

namespace StupidHabitTracker.Models;

public class TimedSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int HabitId { get; set; }
    
    public string HabitName { get; set; } = string.Empty;
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public double DurationSeconds { get; set; }
    
    [Ignore]
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
}
