namespace QuestBoard.Api.Patterns.Chain;

public class QuestValidationContext
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BaseGold { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
    public DateTime? Deadline { get; set; }
    public bool IsUrgent { get; set; }
}
