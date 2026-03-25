namespace QuestBoard.Api.Models.Domain;

public enum QuestStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum QuestDifficulty
{
    Easy,
    Medium,
    Hard,
    Epic,
    Legendary
}

public enum QuestType
{
    Development,
    Design,
    Writing,
    Testing,
    DevOps,
    DataScience
}

public enum PricingType
{
    Fixed,
    Auction,
    Dynamic
}

public enum MatchmakingType
{
    SkillBased,
    RatingBased
}
