namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] Concrete implementor — webhook kanaal
public class WebhookSender : INotificationSender
{
    public string Channel => "Webhook";

    public string Send(string recipient, string subject, string body)
    {
        return $"[WEBHOOK] URL: {recipient} | Payload: {{\"subject\":\"{subject}\",\"body\":\"{body}\"}}";
    }
}
