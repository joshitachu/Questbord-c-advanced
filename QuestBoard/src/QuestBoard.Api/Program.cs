using QuestBoard.Api.Data;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Patterns.Bridge;
using QuestBoard.Api.Patterns.Flyweight;
using QuestBoard.Api.Patterns.Observer;
using QuestBoard.Api.Patterns.Strategy;
using QuestBoard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI + Controllers
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// === Data Store ===
builder.Services.AddSingleton<InMemoryDataStore>();

// === Flyweight Factories (singleton pools) ===
builder.Services.AddSingleton(SkillFactory.Instance);
builder.Services.AddSingleton(BadgeFactory.Instance);

// === Strategy Pattern: Pricing ===
builder.Services.AddSingleton<IPricingStrategy, FixedPricingStrategy>();
builder.Services.AddSingleton<IPricingStrategy, AuctionPricingStrategy>();
builder.Services.AddSingleton<IPricingStrategy, DynamicPricingStrategy>();

// === Strategy Pattern: Matchmaking ===
builder.Services.AddSingleton<IMatchmakingStrategy, SkillBasedMatchmaking>();
builder.Services.AddSingleton<IMatchmakingStrategy, RatingBasedMatchmaking>();

// === Bridge Pattern: Notification Senders ===
builder.Services.AddSingleton<INotificationSender, EmailSender>();
builder.Services.AddSingleton<INotificationSender, PushNotificationSender>();
builder.Services.AddSingleton<INotificationSender, WebhookSender>();

// === Observer Pattern ===
builder.Services.AddSingleton<IQuestEventPublisher, QuestEventPublisher>();
builder.Services.AddSingleton<XpCalculatorSubscriber>();
builder.Services.AddSingleton<AchievementCheckerSubscriber>();
builder.Services.AddSingleton<LeaderboardUpdaterSubscriber>();

// === Services ===
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IFreelancerService, FreelancerService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

var app = builder.Build();

// === Wire Observer Subscribers ===
var publisher = app.Services.GetRequiredService<IQuestEventPublisher>();
publisher.Subscribe(app.Services.GetRequiredService<XpCalculatorSubscriber>());
publisher.Subscribe(app.Services.GetRequiredService<AchievementCheckerSubscriber>());
publisher.Subscribe(app.Services.GetRequiredService<LeaderboardUpdaterSubscriber>());

// === Seed Data ===
SeedData(app.Services);

// === Middleware ===
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "QuestBoard API");
});
app.MapControllers();

// === Debug Endpoint: Flyweight Stats ===
app.MapGet("/api/debug/flyweight-stats", (SkillFactory skillFactory, BadgeFactory badgeFactory) =>
{
    return Results.Ok(new
    {
        Skills = new
        {
            PoolSize = skillFactory.GetSkillCount(),
            Items = skillFactory.GetAllSkills().Select(s => new { s.Name, s.Category })
        },
        Badges = new
        {
            PoolSize = badgeFactory.GetBadgeCount(),
            Items = badgeFactory.GetAllBadges().Select(b => new { b.Name, b.Tier, b.Description })
        },
        Message = "Flyweight pattern: objecten worden gedeeld via factory pools. " +
                  "Meerdere freelancers met dezelfde skill/badge verwijzen naar hetzelfde object in geheugen."
    });
}).WithTags("Debug");

app.Run();

// === Seed Data Method ===
static void SeedData(IServiceProvider services)
{
    var store = services.GetRequiredService<InMemoryDataStore>();
    var skillFactory = services.GetRequiredService<SkillFactory>();
    var badgeFactory = services.GetRequiredService<BadgeFactory>();

    // --- 15 Skills ---
    var skills = new (string Name, string Category)[]
    {
        ("C#", "Backend"), ("ASP.NET Core", "Backend"), ("Entity Framework", "Backend"),
        ("JavaScript", "Frontend"), ("React", "Frontend"), ("TypeScript", "Frontend"),
        ("Python", "Backend"), ("Docker", "DevOps"), ("Kubernetes", "DevOps"),
        ("SQL", "Database"), ("MongoDB", "Database"), ("Azure", "Cloud"),
        ("Git", "Tools"), ("UI/UX Design", "Design"), ("Machine Learning", "Data Science")
    };
    foreach (var (name, category) in skills)
        skillFactory.GetSkill(name, category, $"/icons/skills/{name.ToLower().Replace(" ", "-")}.png");

    // --- 8 Badges ---
    var badges = new (string Name, string Desc, string Tier)[]
    {
        ("Quest Novice", "Complete your first quest", "Bronze"),
        ("Quest Veteran", "Complete 10 quests", "Silver"),
        ("Quest Master", "Complete 25 quests", "Gold"),
        ("Rising Star", "Reach level 5", "Silver"),
        ("Elite Coder", "Reach level 10", "Gold"),
        ("Gold Hoarder", "Earn 5000 gold", "Gold"),
        ("Skill Collector", "Learn 5 or more skills", "Silver"),
        ("Top Rated", "Maintain 4.5+ average rating", "Gold")
    };
    foreach (var (name, desc, tier) in badges)
        badgeFactory.GetBadge(name, desc, tier, $"/icons/badges/{name.ToLower().Replace(" ", "-")}.png");

    // --- 3 Clients ---
    var client1 = new Client { Name = "TechCorp", Company = "TechCorp BV", Balance = 50000 };
    var client2 = new Client { Name = "DesignHub", Company = "DesignHub NL", Balance = 30000 };
    var client3 = new Client { Name = "DataDriven", Company = "DataDriven Inc", Balance = 75000 };
    store.Clients[client1.Id] = client1;
    store.Clients[client2.Id] = client2;
    store.Clients[client3.Id] = client3;

    // --- 5 Freelancers ---
    var freelancers = new Freelancer[]
    {
        new() { Name = "Alice van Dev", Email = "alice@dev.nl", Level = 8, Xp = 7500, Gold = 4200,
                Skills = new List<string> { "C#", "ASP.NET Core", "Entity Framework", "Docker", "SQL" },
                QuestsCompleted = 15, AverageRating = 4.8 },
        new() { Name = "Bob de Builder", Email = "bob@builder.nl", Level = 5, Xp = 4200, Gold = 2800,
                Skills = new List<string> { "JavaScript", "React", "TypeScript", "Git" },
                QuestsCompleted = 8, AverageRating = 4.3 },
        new() { Name = "Carol Coder", Email = "carol@code.nl", Level = 3, Xp = 2100, Gold = 1500,
                Skills = new List<string> { "Python", "Machine Learning", "SQL" },
                QuestsCompleted = 4, AverageRating = 4.6 },
        new() { Name = "Dave DevOps", Email = "dave@ops.nl", Level = 6, Xp = 5800, Gold = 3500,
                Skills = new List<string> { "Docker", "Kubernetes", "Azure", "Git", "Python" },
                QuestsCompleted = 12, AverageRating = 4.1 },
        new() { Name = "Eva Designer", Email = "eva@design.nl", Level = 4, Xp = 3200, Gold = 2200,
                Skills = new List<string> { "UI/UX Design", "React", "TypeScript", "JavaScript" },
                QuestsCompleted = 6, AverageRating = 4.9 }
    };
    foreach (var f in freelancers)
    {
        store.Freelancers[f.Id] = f;
        store.LeaderboardScores[f.Id] = f.Xp + (int)f.Gold;
    }

    // --- 8 Achievements with DSL rules ---
    var achievements = new Achievement[]
    {
        new() { Name = "First Blood", Description = "Complete your first quest",
                BadgeName = "Quest Novice", DslRule = "quests.completed >= 1", XpReward = 100, GoldReward = 50 },
        new() { Name = "Quest Veteran", Description = "Complete 10 quests",
                BadgeName = "Quest Veteran", DslRule = "quests.completed >= 10", XpReward = 500, GoldReward = 250 },
        new() { Name = "Quest Master", Description = "Complete 25 quests with high rating",
                BadgeName = "Quest Master", DslRule = "quests.completed >= 25 and rating.avg >= 4.0", XpReward = 1000, GoldReward = 500 },
        new() { Name = "Rising Star", Description = "Reach level 5",
                BadgeName = "Rising Star", DslRule = "level >= 5", XpReward = 300, GoldReward = 150 },
        new() { Name = "Elite Status", Description = "Reach level 10 or complete 20 quests",
                BadgeName = "Elite Coder", DslRule = "level >= 10 or quests.completed >= 20", XpReward = 750, GoldReward = 400 },
        new() { Name = "Gold Hoarder", Description = "Accumulate 5000 gold",
                BadgeName = "Gold Hoarder", DslRule = "gold >= 5000", XpReward = 400, GoldReward = 0 },
        new() { Name = "Skill Collector", Description = "Learn 5 or more skills",
                BadgeName = "Skill Collector", DslRule = "skills.count >= 5", XpReward = 200, GoldReward = 100 },
        new() { Name = "Top Rated Pro", Description = "Maintain excellent rating with experience",
                BadgeName = "Top Rated", DslRule = "rating.avg >= 4.5 and quests.completed >= 5", XpReward = 600, GoldReward = 300 }
    };
    foreach (var a in achievements)
        store.Achievements[a.Id] = a;

    // --- 5 Quests ---
    var quests = new Quest[]
    {
        new() { Title = "Build REST API", Description = "Ontwikkel een complete REST API met ASP.NET Core",
                ClientId = client1.Id, Difficulty = QuestDifficulty.Medium, Type = QuestType.Development,
                PricingType = PricingType.Fixed, BaseGold = 500, BaseXp = 250,
                RequiredSkills = new List<string> { "C#", "ASP.NET Core", "SQL" } },
        new() { Title = "Frontend Dashboard", Description = "Bouw een React dashboard met real-time data",
                ClientId = client2.Id, Difficulty = QuestDifficulty.Hard, Type = QuestType.Development,
                PricingType = PricingType.Dynamic, BaseGold = 800, BaseXp = 500,
                RequiredSkills = new List<string> { "React", "TypeScript", "JavaScript" },
                IsUrgent = true, Deadline = DateTime.UtcNow.AddDays(2) },
        new() { Title = "ML Model Training", Description = "Train een ML model voor klantvoorspellingen",
                ClientId = client3.Id, Difficulty = QuestDifficulty.Epic, Type = QuestType.DataScience,
                PricingType = PricingType.Auction, BaseGold = 1500, BaseXp = 1000,
                RequiredSkills = new List<string> { "Python", "Machine Learning", "SQL" } },
        new() { Title = "CI/CD Pipeline Setup", Description = "Configureer complete CI/CD pipeline met Docker en K8s",
                ClientId = client1.Id, Difficulty = QuestDifficulty.Hard, Type = QuestType.DevOps,
                PricingType = PricingType.Fixed, BaseGold = 700, BaseXp = 500,
                RequiredSkills = new List<string> { "Docker", "Kubernetes", "Azure" },
                IsFeatured = true },
        new() { Title = "Team Website Redesign", Description = "Herontwerp de bedrijfswebsite als team",
                ClientId = client2.Id, Difficulty = QuestDifficulty.Medium, Type = QuestType.Design,
                PricingType = PricingType.Fixed, BaseGold = 600, BaseXp = 300,
                RequiredSkills = new List<string> { "UI/UX Design", "React", "TypeScript" },
                IsTeamQuest = true, MaxTeamSize = 3, IsFeatured = true }
    };
    foreach (var q in quests)
        store.Quests[q.Id] = q;
}
