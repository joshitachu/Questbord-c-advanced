namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Concrete implementor — push notification kanaal
public class PushNotificationSender : INotificationSender
{
    public string Channel => "Push";

    public string Send(string recipient, string subject, string body)
    {
        return $"[PUSH] Device: {recipient} | Alert: {subject} - {body}";
    }
}
