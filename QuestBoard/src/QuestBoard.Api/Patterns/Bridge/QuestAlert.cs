namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Refined abstractie — quest status notificatie
public class QuestAlert : Notification
{
    public string QuestTitle { get; }
    public string QuestStatus { get; }

    public QuestAlert(INotificationSender sender, string questTitle, string questStatus)
        : base(sender)
    {
        QuestTitle = questTitle;
        QuestStatus = questStatus;
    }

    public override string Notify(string recipient)
    {
        return _sender.Send(recipient, $"Quest Update: {QuestTitle}", $"Quest '{QuestTitle}' status changed to: {QuestStatus}");
    }
}
