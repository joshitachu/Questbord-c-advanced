namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Refined abstractie — achievement unlock notificatie
public class AchievementAlert : Notification
{
    public string AchievementName { get; }
    public string BadgeName { get; }

    public AchievementAlert(INotificationSender sender, string achievementName, string badgeName)
        : base(sender)
    {
        AchievementName = achievementName;
        BadgeName = badgeName;
    }

    public override string Notify(string recipient)
    {
        return _sender.Send(recipient, $"Achievement Unlocked: {AchievementName}", $"You earned the '{BadgeName}' badge!");
    }
}
