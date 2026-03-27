namespace QuestBoard.Api.Models.DTOs;

using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 50 tekens zijn.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mailadres is verplicht.")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    [StringLength(100, ErrorMessage = "E-mailadres mag maximaal 100 tekens zijn.")]
    public string Email { get; set; } = string.Empty;
}

public class AddSkillDto
{
    [Required(ErrorMessage = "Skill naam is verplicht.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Skill naam moet tussen 1 en 50 tekens zijn.")]
    [RegularExpression(@"^[a-zA-Z0-9\s\.\+\#\/\-]+$", ErrorMessage = "Skill naam bevat ongeldige tekens.")]
    public string SkillName { get; set; } = string.Empty;
}

public class CreateAchievementDto
{
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beschrijving is verplicht.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Beschrijving moet tussen 5 en 500 tekens zijn.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Badge naam is verplicht.")]
    [StringLength(50, ErrorMessage = "Badge naam mag maximaal 50 tekens zijn.")]
    public string BadgeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "DSL regel is verplicht.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "DSL regel moet tussen 5 en 200 tekens zijn.")]
    public string DslRule { get; set; } = string.Empty;

    [Range(0, 10000, ErrorMessage = "XP beloning moet tussen 0 en 10.000 liggen.")]
    public int XpReward { get; set; }

    [Range(0, 50000, ErrorMessage = "Gold beloning moet tussen 0 en 50.000 liggen.")]
    public decimal GoldReward { get; set; }
}

public class TestNotificationDto
{
    [Required(ErrorMessage = "Type is verplicht.")]
    [RegularExpression(@"^(quest|achievement)$", ErrorMessage = "Type moet 'quest' of 'achievement' zijn.")]
    public string Type { get; set; } = "quest";

    [Required(ErrorMessage = "Kanaal is verplicht.")]
    [RegularExpression(@"^(email|push|webhook)$", ErrorMessage = "Kanaal moet 'email', 'push' of 'webhook' zijn.")]
    public string Channel { get; set; } = "email";

    [Required(ErrorMessage = "Bericht is verplicht.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Bericht moet tussen 1 en 500 tekens zijn.")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ontvanger is verplicht.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Ontvanger moet tussen 1 en 100 tekens zijn.")]
    public string Recipient { get; set; } = string.Empty;
}
