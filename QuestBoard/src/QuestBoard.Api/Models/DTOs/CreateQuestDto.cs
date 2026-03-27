namespace QuestBoard.Api.Models.DTOs;

using System.ComponentModel.DataAnnotations;
using QuestBoard.Api.Models.Domain;

public class CreateQuestDto
{
    [Required(ErrorMessage = "Titel is verplicht.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Titel moet tussen 3 en 100 tekens zijn.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beschrijving is verplicht.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Beschrijving moet tussen 10 en 1000 tekens zijn.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClientId is verplicht.")]
    public Guid ClientId { get; set; }

    public QuestDifficulty Difficulty { get; set; } = QuestDifficulty.Medium;

    public QuestType Type { get; set; } = QuestType.Development;

    public PricingType PricingType { get; set; } = PricingType.Fixed;

    [Range(0, 100000, ErrorMessage = "BaseGold moet tussen 0 en 100.000 liggen.")]
    public decimal BaseGold { get; set; }

    [Range(0, 50000, ErrorMessage = "BaseXp moet tussen 0 en 50.000 liggen.")]
    public int BaseXp { get; set; }

    public List<string> RequiredSkills { get; set; } = new();

    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTeamQuest { get; set; }

    [Range(1, 10, ErrorMessage = "MaxTeamSize moet tussen 1 en 10 liggen.")]
    public int MaxTeamSize { get; set; } = 1;

    public DateTime? Deadline { get; set; }
}
