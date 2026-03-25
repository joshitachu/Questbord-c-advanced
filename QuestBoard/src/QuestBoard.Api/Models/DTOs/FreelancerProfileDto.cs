namespace QuestBoard.Api.Models.DTOs;

public class FreelancerProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public decimal Gold { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Badges { get; set; } = new();
    public int QuestsCompleted { get; set; }
    public double AverageRating { get; set; }
}

public class CreateFreelancerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AddSkillDto
{
    public string SkillName { get; set; } = string.Empty;
}

public class CreateAchievementDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BadgeName { get; set; } = string.Empty;
    public string DslRule { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public decimal GoldReward { get; set; }
}

public class TestNotificationDto
{
    public string Type { get; set; } = "quest";
    public string Channel { get; set; } = "email";
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
}
