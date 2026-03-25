namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Abstractie-kant van het Bridge pattern
// Notification (abstractie) x INotificationSender (implementatie)
public abstract class Notification
{
    protected readonly INotificationSender _sender;

    protected Notification(INotificationSender sender)
    {
        _sender = sender;
    }

    public abstract string Notify(string recipient);
    public string SenderChannel => _sender.Channel;
}
