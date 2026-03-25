namespace QuestBoard.Api.Patterns.Observer;

// [PATTERN: Observer] — Concrete subscriber
// Verwerkt XP, Gold, Level en QuestsCompleted na een quest completion
public class XpCalculatorSubscriber : IQuestEventSubscriber
{
    public string Name => "XpCalculator";

    public void OnQuestCompleted(QuestCompletedEvent evt)
    {
        var freelancer = evt.Freelancer;

        freelancer.Xp += evt.XpEarned;
        freelancer.Gold += evt.GoldEarned;
        freelancer.QuestsCompleted++;

        // Simple level formula: level = 1 + (Xp / 1000)
        var newLevel = 1 + (freelancer.Xp / 1000);
        if (newLevel != freelancer.Level)
        {
            freelancer.Level = newLevel;
        }
    }
}
