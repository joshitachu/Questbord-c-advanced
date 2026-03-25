namespace QuestBoard.Api.Models.Domain;

public class Quest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? AssignedFreelancerId { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Open;
    public QuestDifficulty Difficulty { get; set; } = QuestDifficulty.Medium;
    public QuestType Type { get; set; } = QuestType.Development;
    public PricingType PricingType { get; set; } = PricingType.Fixed;
    public decimal BaseGold { get; set; }
    public int BaseXp { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTeamQuest { get; set; }
    public int MaxTeamSize { get; set; } = 1;
    public List<Guid> TeamMemberIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? Deadline { get; set; }
}
