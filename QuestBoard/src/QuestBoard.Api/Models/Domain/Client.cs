namespace QuestBoard.Api.Models.Domain;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public List<Guid> PostedQuestIds { get; set; } = new();
}
