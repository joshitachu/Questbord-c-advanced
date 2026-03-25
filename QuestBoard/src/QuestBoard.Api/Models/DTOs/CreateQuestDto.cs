namespace QuestBoard.Api.Models.DTOs;

using QuestBoard.Api.Models.Domain;

public class CreateQuestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
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
    public DateTime? Deadline { get; set; }
}
