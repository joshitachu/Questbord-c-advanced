using QuestBoard.Api.Data;
using QuestBoard.Api.Hubs;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Patterns.Bridge;
using QuestBoard.Api.Patterns.Concurrency;
using QuestBoard.Api.Patterns.Creational;
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

// === Creational: Singleton ===
builder.Services.AddSingleton(GameConfigurationManager.Instance);

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
builder.Services.AddSingleton<SignalRBroadcasterSubscriber>();

// === Concurrency: Monitor ===
builder.Services.AddSingleton<QuestAcceptanceLock>();

// === Concurrency: Producer-Consumer ===
builder.Services.AddSingleton<IEventQueue<QuestCompletedEvent>>(new EventQueue<QuestCompletedEvent>(100));
builder.Services.AddHostedService<EventProcessorService>();

// === SignalR ===
builder.Services.AddSignalR();

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
publisher.Subscribe(app.Services.GetRequiredService<SignalRBroadcasterSubscriber>());

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

// === SignalR Hub ===
app.MapHub<QuestBoardHub>("/hub/questboard");

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

    // --- 5 Quests (gebouwd met QuestBuilder) ---
    var quests = new Quest[]
    {
        QuestBuilder.Create()
            .WithTitle("Build REST API")
            .WithDescription("Ontwikkel een complete REST API met ASP.NET Core")
            .ForClient(client1.Id)
            .WithDifficulty(QuestDifficulty.Medium)
            .WithType(QuestType.Development)
            .WithPricing(PricingType.Fixed, 500)
            .WithBaseXp(250)
            .WithSkills("C#", "ASP.NET Core", "SQL")
            .Build(),
        QuestBuilder.Create()
            .WithTitle("Frontend Dashboard")
            .WithDescription("Bouw een React dashboard met real-time data")
            .ForClient(client2.Id)
            .WithDifficulty(QuestDifficulty.Hard)
            .WithType(QuestType.Development)
            .WithPricing(PricingType.Dynamic, 800)
            .WithBaseXp(500)
            .WithSkills("React", "TypeScript", "JavaScript")
            .AsUrgent(DateTime.UtcNow.AddDays(2))
            .Build(),
        QuestBuilder.Create()
            .WithTitle("ML Model Training")
            .WithDescription("Train een ML model voor klantvoorspellingen")
            .ForClient(client3.Id)
            .WithDifficulty(QuestDifficulty.Epic)
            .WithType(QuestType.DataScience)
            .WithPricing(PricingType.Auction, 1500)
            .WithBaseXp(1000)
            .WithSkills("Python", "Machine Learning", "SQL")
            .Build(),
        QuestBuilder.Create()
            .WithTitle("CI/CD Pipeline Setup")
            .WithDescription("Configureer complete CI/CD pipeline met Docker en K8s")
            .ForClient(client1.Id)
            .WithDifficulty(QuestDifficulty.Hard)
            .WithType(QuestType.DevOps)
            .WithPricing(PricingType.Fixed, 700)
            .WithBaseXp(500)
            .WithSkills("Docker", "Kubernetes", "Azure")
            .AsFeatured()
            .Build(),
        QuestBuilder.Create()
            .WithTitle("Team Website Redesign")
            .WithDescription("Herontwerp de bedrijfswebsite als team")
            .ForClient(client2.Id)
            .WithDifficulty(QuestDifficulty.Medium)
            .WithType(QuestType.Design)
            .WithPricing(PricingType.Fixed, 600)
            .WithBaseXp(300)
            .WithSkills("UI/UX Design", "React", "TypeScript")
            .AsTeamQuest(3)
            .AsFeatured()
            .Build()
    };
    foreach (var q in quests)
        store.Quests[q.Id] = q;
}
