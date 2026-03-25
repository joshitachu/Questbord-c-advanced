namespace QuestBoard.Api.Patterns.Bridge;

// [PATTERN: Bridge] — Structural pattern
// Scheidt notificatie-abstractie (wat) van implementatie (hoe)
// Voorkomt class-explosie: 2 types × 3 kanalen = 6 combinaties zonder 6 klassen
public interface INotificationSender
{
    string Channel { get; }
    string Send(string recipient, string subject, string body);
}
