namespace QuestBoard.Api.Models.DTOs;

using QuestBoard.Api.Models.Domain;

public class QuestResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestStatus Status { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    public QuestType Type { get; set; }
    public decimal Gold { get; set; }
    public int Xp { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTeamQuest { get; set; }
    public int MaxTeamSize { get; set; }
    public Guid? AssignedFreelancerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
}
