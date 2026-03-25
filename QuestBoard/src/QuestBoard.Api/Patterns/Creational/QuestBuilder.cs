using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Patterns.Creational;

// [PATTERN: Builder] — Creational pattern
// Fluent builder voor Quest objecten. Vervangt complexe object initializers
// met een leesbare, stapsgewijze constructie inclusief validatie en defaults.
public class QuestBuilder
{
    private readonly Quest _quest = new();
    private bool _autoBaseXp = true;

    private QuestBuilder() { }

    public static QuestBuilder Create() => new();

    public QuestBuilder WithTitle(string title)
    {
        _quest.Title = title;
        return this;
    }

    public QuestBuilder WithDescription(string description)
    {
        _quest.Description = description;
        return this;
    }

    public QuestBuilder ForClient(Guid clientId)
    {
        _quest.ClientId = clientId;
        return this;
    }

    public QuestBuilder WithDifficulty(QuestDifficulty difficulty)
    {
        _quest.Difficulty = difficulty;
        return this;
    }

    public QuestBuilder WithType(QuestType type)
    {
        _quest.Type = type;
        return this;
    }

    public QuestBuilder WithPricing(PricingType pricingType, decimal baseGold)
    {
        _quest.PricingType = pricingType;
        _quest.BaseGold = baseGold;
        return this;
    }

    public QuestBuilder WithBaseXp(int baseXp)
    {
        _quest.BaseXp = baseXp;
        _autoBaseXp = false;
        return this;
    }

    public QuestBuilder WithSkills(params string[] skills)
    {
        _quest.RequiredSkills = skills.ToList();
        return this;
    }

    public QuestBuilder AsUrgent(DateTime? deadline = null)
    {
        _quest.IsUrgent = true;
        _quest.Deadline = deadline;
        return this;
    }

    public QuestBuilder AsFeatured()
    {
        _quest.IsFeatured = true;
        return this;
    }

    public QuestBuilder AsTeamQuest(int maxSize = 3)
    {
        _quest.IsTeamQuest = true;
        _quest.MaxTeamSize = Math.Max(maxSize, GameConfigurationManager.Instance.MinTeamSize);
        return this;
    }

    public QuestBuilder WithDeadline(DateTime deadline)
    {
        _quest.Deadline = deadline;
        return this;
    }

    public Quest Build()
    {
        // Validatie
        if (string.IsNullOrWhiteSpace(_quest.Title))
            throw new InvalidOperationException("Quest title is verplicht.");

        if (_quest.ClientId == Guid.Empty)
            throw new InvalidOperationException("Quest clientId is verplicht.");

        // Defaults vanuit Singleton configuratie
        var config = GameConfigurationManager.Instance;

        if (_autoBaseXp)
            _quest.BaseXp = config.GetBaseXp(_quest.Difficulty);

        if (!_quest.IsTeamQuest)
            _quest.MaxTeamSize = config.DefaultMaxTeamSize;

        return _quest;
    }
}
