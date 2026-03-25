using Microsoft.AspNetCore.Mvc;
using QuestBoard.Api.Data;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Patterns.Bridge;

namespace QuestBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly InMemoryDataStore _store;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationsController(InMemoryDataStore store, IEnumerable<INotificationSender> senders)
    {
        _store = store;
        _senders = senders;
    }

    /// <summary>Bridge pattern demo: test notificaties via verschillende kanalen</summary>
    [HttpPost("test")]
    public IActionResult TestNotification([FromBody] TestNotificationDto dto)
    {
        // [PATTERN: Bridge] — Selecteer sender en abstractie onafhankelijk
        var sender = _senders.FirstOrDefault(s =>
            s.Channel.Equals(dto.Channel, StringComparison.OrdinalIgnoreCase))
            ?? _senders.First();

        Notification notification = dto.Type.ToLower() switch
        {
            "achievement" => new AchievementAlert(sender, dto.Message, "TestBadge"),
            _ => new QuestAlert(sender, dto.Message, "TestStatus")
        };

        var result = notification.Notify(dto.Recipient);
        _store.NotificationLog.Add(result);

        return Ok(new
        {
            Channel = sender.Channel,
            Type = dto.Type,
            Result = result,
            TotalNotificationsSent = _store.NotificationLog.Count
        });
    }
}
