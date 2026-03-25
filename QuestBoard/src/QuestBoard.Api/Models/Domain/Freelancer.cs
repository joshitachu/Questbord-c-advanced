namespace QuestBoard.Api.Models.Domain;

public class Freelancer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
    public decimal Gold { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Badges { get; set; } = new();
    public List<Guid> CompletedQuestIds { get; set; } = new();
    public int QuestsCompleted { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
