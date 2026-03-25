using QuestBoard.Api.Patterns.Bridge;

namespace QuestBoard.Tests.Patterns;

public class BridgeTests
{
    [Fact]
    public void QuestAlert_WithEmailSender_FormatsCorrectly()
    {
        // Arrange
        var sender = new EmailSender();
        var alert = new QuestAlert(sender, "Slay the Dragon", "Completed");

        // Act
        var result = alert.Notify("hero@questboard.com");

        // Assert
        Assert.Equal(
            "[EMAIL] To: hero@questboard.com | Subject: Quest Update: Slay the Dragon | Body: Quest 'Slay the Dragon' status changed to: Completed",
            result);
    }

    [Fact]
    public void QuestAlert_WithPushSender_FormatsCorrectly()
    {
        // Arrange
        var sender = new PushNotificationSender();
        var alert = new QuestAlert(sender, "Slay the Dragon", "Completed");

        // Act
        var result = alert.Notify("device-token-123");

        // Assert
        Assert.Equal(
            "[PUSH] Device: device-token-123 | Alert: Quest Update: Slay the Dragon - Quest 'Slay the Dragon' status changed to: Completed",
            result);
    }

    [Fact]
    public void AchievementAlert_WithWebhookSender_FormatsCorrectly()
    {
        // Arrange
        var sender = new WebhookSender();
        var alert = new AchievementAlert(sender, "Dragon Slayer", "Golden Shield");

        // Act
        var result = alert.Notify("https://hooks.example.com/notify");

        // Assert
        Assert.Equal(
            "[WEBHOOK] URL: https://hooks.example.com/notify | Payload: {\"subject\":\"Achievement Unlocked: Dragon Slayer\",\"body\":\"You earned the 'Golden Shield' badge!\"}",
            result);
    }

    [Fact]
    public void SameAlert_DifferentSenders_DifferentOutput()
    {
        // Arrange
        var emailSender = new EmailSender();
        var pushSender = new PushNotificationSender();
        var emailAlert = new QuestAlert(emailSender, "Slay the Dragon", "Completed");
        var pushAlert = new QuestAlert(pushSender, "Slay the Dragon", "Completed");

        // Act
        var emailResult = emailAlert.Notify("hero@questboard.com");
        var pushResult = pushAlert.Notify("hero@questboard.com");

        // Assert
        Assert.NotEqual(emailResult, pushResult);
    }

    [Fact]
    public void SenderChannel_ReturnsCorrectChannel()
    {
        // Arrange & Act
        var emailAlert = new QuestAlert(new EmailSender(), "Test", "Active");
        var pushAlert = new QuestAlert(new PushNotificationSender(), "Test", "Active");
        var webhookAlert = new AchievementAlert(new WebhookSender(), "Test", "Star");

        // Assert
        Assert.Equal("Email", emailAlert.SenderChannel);
        Assert.Equal("Push", pushAlert.SenderChannel);
        Assert.Equal("Webhook", webhookAlert.SenderChannel);
    }
}
