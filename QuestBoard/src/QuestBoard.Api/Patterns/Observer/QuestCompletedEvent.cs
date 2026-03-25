namespace QuestBoard.Api.Patterns.Observer;

using QuestBoard.Api.Models.Domain;

// [PATTERN: Observer] — Behavioral pattern
// Quest completion event data die naar alle subscribers wordt gestuurd
public class QuestCompletedEvent
{
    public Quest Quest { get; }
    public Freelancer Freelancer { get; }
    public decimal GoldEarned { get; }
    public int XpEarned { get; }

    public QuestCompletedEvent(Quest quest, Freelancer freelancer, decimal goldEarned, int xpEarned)
    {
        Quest = quest;
        Freelancer = freelancer;
        GoldEarned = goldEarned;
        XpEarned = xpEarned;
    }
}
