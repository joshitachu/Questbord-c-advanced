namespace QuestBoard.Api.Models.Domain;

public class Achievement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BadgeName { get; set; } = string.Empty;
    public string DslRule { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public decimal GoldReward { get; set; }
}
