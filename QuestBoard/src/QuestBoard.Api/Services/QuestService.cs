using QuestBoard.Api.Data;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Patterns.Concurrency;
using QuestBoard.Api.Patterns.Creational;
using QuestBoard.Api.Patterns.Decorator;
using QuestBoard.Api.Patterns.Observer;
using QuestBoard.Api.Patterns.Strategy;

namespace QuestBoard.Api.Services;

public class QuestService : IQuestService
{
    private readonly InMemoryDataStore _store;
    private readonly IEnumerable<IPricingStrategy> _pricingStrategies;
    private readonly IEnumerable<IMatchmakingStrategy> _matchmakingStrategies;
    private readonly IQuestEventPublisher _eventPublisher;
    private readonly QuestAcceptanceLock _acceptanceLock;
    private readonly IEventQueue<QuestCompletedEvent> _eventQueue;

    public QuestService(
        InMemoryDataStore store,
        IEnumerable<IPricingStrategy> pricingStrategies,
        IEnumerable<IMatchmakingStrategy> matchmakingStrategies,
        IQuestEventPublisher eventPublisher,
        QuestAcceptanceLock acceptanceLock,
        IEventQueue<QuestCompletedEvent> eventQueue)
    {
        _store = store;
        _pricingStrategies = pricingStrategies;
        _matchmakingStrategies = matchmakingStrategies;
        _eventPublisher = eventPublisher;
        _acceptanceLock = acceptanceLock;
        _eventQueue = eventQueue;
    }

    public QuestResponseDto CreateQuest(CreateQuestDto dto)
    {
        // [PATTERN: Strategy] — Selecteer pricing strategy op basis van PricingType
        var strategy = _pricingStrategies.FirstOrDefault(s =>
            s.Name.Equals(dto.PricingType.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? _pricingStrategies.First();

        var calculatedGold = strategy.CalculatePrice(dto.BaseGold, (int)dto.Difficulty, dto.Deadline);

        // [PATTERN: Builder] — Gebruik QuestBuilder voor leesbare quest constructie
        var builder = QuestBuilder.Create()
            .WithTitle(dto.Title)
            .WithDescription(dto.Description)
            .ForClient(dto.ClientId)
            .WithDifficulty(dto.Difficulty)
            .WithType(dto.Type)
            .WithPricing(dto.PricingType, calculatedGold)
            .WithSkills(dto.RequiredSkills.ToArray());

        if (dto.BaseXp > 0)
            builder.WithBaseXp(dto.BaseXp);

        if (dto.IsUrgent)
            builder.AsUrgent(dto.Deadline);
        else if (dto.Deadline.HasValue)
            builder.WithDeadline(dto.Deadline.Value);

        if (dto.IsFeatured)
            builder.AsFeatured();

        if (dto.IsTeamQuest)
            builder.AsTeamQuest(dto.MaxTeamSize);

        var quest = builder.Build();

        _store.Quests[quest.Id] = quest;
        return MapToResponse(quest);
    }

    public List<QuestResponseDto> GetAllQuests()
    {
        return _store.Quests.Values
            .OrderByDescending(q => q.CreatedAt)
            .Select(MapToResponse)
            .ToList();
    }

    public QuestResponseDto? GetQuest(Guid id)
    {
        return _store.Quests.TryGetValue(id, out var quest) ? MapToResponse(quest) : null;
    }

    public QuestResponseDto? AcceptQuest(Guid questId, Guid freelancerId)
    {
        // [PATTERN: Monitor] — Per-quest lock voorkomt race condition op quest.Status
        return _acceptanceLock.ExecuteWithLock(questId, () =>
        {
            if (!_store.Quests.TryGetValue(questId, out var quest)) return null;
            if (!_store.Freelancers.ContainsKey(freelancerId)) return null;
            if (quest.Status != QuestStatus.Open) return null;

            if (quest.IsTeamQuest && quest.TeamMemberIds.Count < quest.MaxTeamSize)
            {
                quest.TeamMemberIds.Add(freelancerId);
                if (quest.AssignedFreelancerId == null)
                    quest.AssignedFreelancerId = freelancerId;
            }
            else
            {
                quest.AssignedFreelancerId = freelancerId;
            }

            quest.Status = QuestStatus.InProgress;
            return MapToResponse(quest);
        });
    }

    public QuestResponseDto? CompleteQuest(Guid questId)
    {
        if (!_store.Quests.TryGetValue(questId, out var quest)) return null;
        if (quest.AssignedFreelancerId == null) return null;
        if (!_store.Freelancers.TryGetValue(quest.AssignedFreelancerId.Value, out var freelancer)) return null;

        quest.Status = QuestStatus.Completed;
        quest.CompletedAt = DateTime.UtcNow;

        // [PATTERN: Decorator] — Bouw quest behavior met decorators
        var behavior = BuildQuestBehavior(quest);
        var finalGold = behavior.CalculateGold(quest.BaseGold);
        var finalXp = behavior.CalculateXp(quest.BaseXp);

        // [PATTERN: Producer-Consumer] — Enqueue event voor async verwerking door BackgroundService
        var evt = new QuestCompletedEvent(quest, freelancer, finalGold, finalXp);
        _eventQueue.EnqueueAsync(evt).AsTask().Wait();

        // [PATTERN: Monitor] — Cleanup lock na quest completion
        _acceptanceLock.TryRemoveLock(questId);

        return MapToResponse(quest);
    }

    public List<(FreelancerProfileDto Freelancer, double Score)> GetMatches(Guid questId, string strategy = "SkillBased")
    {
        if (!_store.Quests.TryGetValue(questId, out var quest)) return new();

        // [PATTERN: Strategy] — Selecteer matchmaking strategy
        var matchmaker = _matchmakingStrategies.FirstOrDefault(s =>
            s.Name.Equals(strategy, StringComparison.OrdinalIgnoreCase))
            ?? _matchmakingStrategies.First();

        var availableFreelancers = _store.Freelancers.Values
            .Where(f => !quest.TeamMemberIds.Contains(f.Id) && f.Id != quest.AssignedFreelancerId);

        return matchmaker.FindMatches(quest, availableFreelancers)
            .Select(m => (MapFreelancer(m.Freelancer), m.Score))
            .ToList();
    }

    // [PATTERN: Decorator] — Stapelt decorators op basis van quest flags
    private IQuestBehavior BuildQuestBehavior(Quest quest)
    {
        IQuestBehavior behavior = new BaseQuestBehavior();

        if (quest.IsUrgent)
            behavior = new UrgentQuestDecorator(behavior);
        if (quest.IsFeatured)
            behavior = new FeaturedQuestDecorator(behavior);
        if (quest.IsTeamQuest)
            behavior = new TeamQuestDecorator(behavior, quest.MaxTeamSize);

        return behavior;
    }

    // [PATTERN: Singleton] — Delegeer naar GameConfigurationManager
    private int CalculateBaseXp(QuestDifficulty difficulty) =>
        GameConfigurationManager.Instance.GetBaseXp(difficulty);

    private QuestResponseDto MapToResponse(Quest quest)
    {
        var behavior = BuildQuestBehavior(quest);
        return new QuestResponseDto
        {
            Id = quest.Id,
            Title = quest.Title,
            Description = behavior.GetDescription(quest.Description),
            Status = quest.Status,
            Difficulty = quest.Difficulty,
            Type = quest.Type,
            Gold = behavior.CalculateGold(quest.BaseGold),
            Xp = behavior.CalculateXp(quest.BaseXp),
            RequiredSkills = quest.RequiredSkills,
            Tags = behavior.GetTags(),
            IsUrgent = quest.IsUrgent,
            IsFeatured = quest.IsFeatured,
            IsTeamQuest = quest.IsTeamQuest,
            MaxTeamSize = behavior.GetMaxTeamSize(quest.MaxTeamSize),
            AssignedFreelancerId = quest.AssignedFreelancerId,
            CreatedAt = quest.CreatedAt,
            Deadline = quest.Deadline
        };
    }

    private FreelancerProfileDto MapFreelancer(Freelancer f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Email = f.Email,
        Level = f.Level,
        Xp = f.Xp,
        Gold = f.Gold,
        Skills = f.Skills,
        Badges = f.Badges,
        QuestsCompleted = f.QuestsCompleted,
        AverageRating = f.AverageRating
    };
}
