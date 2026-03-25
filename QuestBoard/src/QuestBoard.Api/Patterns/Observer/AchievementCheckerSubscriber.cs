using QuestBoard.Api.Data;
using QuestBoard.Api.Patterns.Bridge;
using QuestBoard.Api.Patterns.Flyweight;
using QuestBoard.Api.Patterns.Interpreter;

namespace QuestBoard.Api.Patterns.Observer;

// [PATTERN: Observer] — Concrete subscriber
// Controleert na quest completion of de freelancer nieuwe achievements heeft ontgrendeld
// Maakt gebruik van Interpreter (DSL evaluatie), Bridge (notificaties) en Flyweight (badges)
public class AchievementCheckerSubscriber : IQuestEventSubscriber
{
    private readonly InMemoryDataStore _dataStore;
    private readonly BadgeFactory _badgeFactory;

    public string Name => "AchievementChecker";

    public AchievementCheckerSubscriber(InMemoryDataStore dataStore, BadgeFactory badgeFactory)
    {
        _dataStore = dataStore;
        _badgeFactory = badgeFactory;
    }

    public void OnQuestCompleted(QuestCompletedEvent evt)
    {
        var freelancer = evt.Freelancer;

        // Build context from freelancer stats
        var properties = new Dictionary<string, double>
        {
            ["quests.completed"] = freelancer.QuestsCompleted,
            ["rating.avg"] = freelancer.AverageRating,
            ["level"] = freelancer.Level,
            ["gold"] = (double)freelancer.Gold,
            ["xp"] = freelancer.Xp,
            ["skills.count"] = freelancer.Skills.Count,
            ["badges.count"] = freelancer.Badges.Count
        };

        var context = new AchievementContext(properties);

        // Check all achievements
        foreach (var achievement in _dataStore.Achievements.Values)
        {
            // Parse DSL rule and evaluate against context
            var expression = AchievementRuleParser.Parse(achievement.DslRule);
            var achieved = expression.Interpret(context);

            if (achieved && !freelancer.Badges.Contains(achievement.BadgeName))
            {
                // Add badge using Flyweight factory
                var badge = _badgeFactory.GetBadge(
                    achievement.BadgeName,
                    achievement.Description);

                freelancer.Badges.Add(badge.Name);

                // Create notification using Bridge pattern
                var alert = new AchievementAlert(
                    new EmailSender(),
                    achievement.Name,
                    achievement.BadgeName);

                var message = alert.Notify(freelancer.Email);
                _dataStore.NotificationLog.Add(message);
            }
        }
    }
}
