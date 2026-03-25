namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Concrete implementor — email kanaal
public class EmailSender : INotificationSender
{
    public string Channel => "Email";

    public string Send(string recipient, string subject, string body)
    {
        return $"[EMAIL] To: {recipient} | Subject: {subject} | Body: {body}";
    }
}
