# QuestBoard - Gamified Freelance Platform

QuestBoard is een ASP.NET Core Web API die een gamified freelance-marktplaats simuleert. Freelancers nemen quests aan, verdienen XP en Gold, stijgen in level, en ontgrendelen achievements. Het project demonstreert **11 design patterns** in een samenhangend, werkend systeem.

## Vereisten

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

Geen database nodig - de applicatie gebruikt in-memory opslag met automatische seed data.

## Opstarten

```bash
dotnet run --project src/QuestBoard.Api
```

De applicatie start op:
- **Frontend**: http://localhost:5284
- **Swagger UI**: http://localhost:5284/swagger/ui
- **SignalR Hub**: ws://localhost:5284/hub/questboard

## Tests draaien

```bash
dotnet test
```

## Projectstructuur

```
src/QuestBoard.Api/
├── Controllers/              # API endpoints
├── Data/                     # InMemoryDataStore
├── Exceptions/               # Custom exception hierarchy
├── Hubs/                     # SignalR real-time communicatie
├── Middleware/                # Global exception handling
├── Models/
│   ├── Domain/               # Quest, Freelancer, Achievement, Client, Enums
│   └── DTOs/                 # CreateQuestDto, FreelancerProfileDto, QuestResponseDto
├── Patterns/
│   ├── Bridge/               # Notification abstractie x kanaal
│   ├── Command/              # Quest lifecycle commands + undo history
│   ├── Concurrency/          # Monitor + Producer-Consumer
│   ├── Creational/           # Singleton + Builder
│   ├── Decorator/            # Quest modifiers (Urgent/Featured/Team)
│   ├── Flyweight/            # Skill/Badge pools
│   ├── Interpreter/          # Achievement DSL parser
│   ├── Observer/             # Event publisher + 4 subscribers
│   └── Strategy/             # Pricing + Matchmaking algoritmes
├── Services/                 # Business logic (QuestService, etc.)
├── wwwroot/index.html        # Frontend (single-page)
└── Program.cs                # DI registratie + seed data

tests/QuestBoard.Tests/
└── Patterns/                 # Unit tests per pattern
```



## Design Patterns

### 1. Singleton (Creational)

**Probleem:** Configuratiewaarden (XP per difficulty, gold multipliers, team sizes) staan verspreid over meerdere klassen als magic numbers.

**Oplossing:** `GameConfigurationManager` centraliseert alle game-instellingen in één thread-safe instantie via `Lazy<T>`.

**Bestanden:**
- `Patterns/Creational/GameConfigurationManager.cs`

**Code:**
```csharp
public sealed class GameConfigurationManager
{
    private static readonly Lazy<GameConfigurationManager> _instance =
        new(() => new GameConfigurationManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static GameConfigurationManager Instance => _instance.Value;

    private GameConfigurationManager()
    {
        BaseXpByDifficulty = new Dictionary<QuestDifficulty, int>
        {
            { QuestDifficulty.Easy, 100 },
            { QuestDifficulty.Medium, 250 },
            { QuestDifficulty.Hard, 500 },
            { QuestDifficulty.Epic, 1000 },
            { QuestDifficulty.Legendary, 2000 }
        };
    }

    public int XpPerLevel { get; } = 1000;
    public decimal UrgentGoldMultiplier { get; } = 1.5m;
    public int MinTeamSize { get; } = 2;
    // ...meer configuratie
}
```

**Gebruikt in:** `QuestBuilder.Build()` haalt defaults op via `GameConfigurationManager.Instance`.


### 2. Builder (Creational)

**Probleem:** Quest objecten hebben veel optionele velden (urgency, team, featured, deadline). Een constructor met 15 parameters is onleesbaar en foutgevoelig.

**Oplossing:** `QuestBuilder` biedt een fluent API voor stapsgewijze constructie met validatie en automatische defaults vanuit de Singleton.

**Bestanden:**
- `Patterns/Creational/QuestBuilder.cs`

**Code:**
```csharp
// Gebruik in Program.cs (seed data):
var quest = QuestBuilder.Create()
    .WithTitle("Build REST API")
    .WithDescription("Ontwikkel een complete REST API")
    .ForClient(client1.Id)
    .WithDifficulty(QuestDifficulty.Medium)
    .WithType(QuestType.Development)
    .WithPricing(PricingType.Fixed, 500)
    .WithSkills("C#", "ASP.NET Core", "SQL")
    .AsUrgent(DateTime.UtcNow.AddDays(2))
    .Build();
```

`Build()` valideert verplichte velden en vult automatisch base XP in vanuit de Singleton:
```csharp
public Quest Build()
{
    if (string.IsNullOrWhiteSpace(_quest.Title))
        throw new InvalidOperationException("Quest title is verplicht.");

    var config = GameConfigurationManager.Instance;
    if (_autoBaseXp)
        _quest.BaseXp = config.GetBaseXp(_quest.Difficulty);

    return _quest;
}
```

**Gebruikt in:** `QuestService.CreateQuest()` en `Program.cs` seed data.


### 3. Strategy (Behavioral)

**Probleem:** Verschillende quests gebruiken verschillende prijs- en matchmaking-algoritmes. Hardcoden van if/else-ketens maakt de code moeilijk uitbreidbaar.

**Oplossing:** Verwisselbare algoritmes achter een interface. Nieuwe strategieen toevoegen vereist alleen een nieuwe klasse, geen bestaande code wijzigen.

**Bestanden:**
- `Patterns/Strategy/IPricingStrategy.cs` - interface
- `Patterns/Strategy/FixedPricingStrategy.cs` - retourneert de basisprijs ongewijzigd
- `Patterns/Strategy/AuctionPricingStrategy.cs` - 20% korting (veiling startprijs)
- `Patterns/Strategy/DynamicPricingStrategy.cs` - prijs op basis van difficulty + deadline urgentie
- `Patterns/Strategy/IMatchmakingStrategy.cs` - interface
- `Patterns/Strategy/SkillBasedMatchmaking.cs` - score op percentage matchende skills
- `Patterns/Strategy/RatingBasedMatchmaking.cs` - score op rating (80%) + skill overlap (20%)

**Code (DynamicPricingStrategy):**
```csharp
public class DynamicPricingStrategy : IPricingStrategy
{
    public string Name => "Dynamic";

    private static readonly Dictionary<int, decimal> DifficultyMultipliers = new()
    {
        { 0, 0.8m }, { 1, 1.0m }, { 2, 1.3m }, { 3, 1.6m }, { 4, 2.0m }
    };

    public decimal CalculatePrice(decimal basePrice, int difficulty, DateTime? deadline)
    {
        var multiplier = DifficultyMultipliers.GetValueOrDefault(difficulty, 1.0m);
        var price = basePrice * multiplier;

        // Deadline binnen 3 dagen? 25% toeslag
        if (deadline.HasValue && (deadline.Value - DateTime.UtcNow).TotalDays <= 3)
            price *= 1.25m;

        return price;
    }
}
```

**Gebruikt in:** `QuestService.CreateQuest()` selecteert pricing strategy op basis van `PricingType`. `QuestService.GetMatches()` selecteert matchmaking strategy op basis van query parameter.

---

### 4. Decorator (Structural)

**Probleem:** Quests kunnen modifiers hebben (Urgent, Featured, Team) die gold, XP en tags aanpassen. Deze modifiers moeten combineerbaar zijn - een quest kan tegelijk Urgent + Featured + Team zijn.

**Oplossing:** Decorators die `IQuestBehavior` wrappen. Elke decorator past berekeningen aan en delegeert naar de inner decorator. Stapelbaar zonder de basis Quest klasse te wijzigen.

**Bestanden:**
- `Patterns/Decorator/IQuestBehavior.cs` - interface
- `Patterns/Decorator/BaseQuestBehavior.cs` - basis (geen modifiers)
- `Patterns/Decorator/QuestDecorator.cs` - abstracte decorator
- `Patterns/Decorator/UrgentQuestDecorator.cs` - 1.5x gold, 2x XP
- `Patterns/Decorator/FeaturedQuestDecorator.cs` - 1.2x gold, +50 XP
- `Patterns/Decorator/TeamQuestDecorator.cs` - gold x teamSize, XP x 0.8

**Code (stapeling in QuestService):**
```csharp
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

// Voorbeeld: een Urgent + Featured quest
// BaseGold = 500
// → UrgentDecorator: 500 * 1.5 = 750
// → FeaturedDecorator: 750 * 1.2 = 900
```

**Gebruikt in:** `QuestService.CompleteQuest()` en `QuestService.MapToResponse()` - elke keer dat gold/XP/tags worden berekend.

---

### 5. Observer (Behavioral)

**Probleem:** Na het voltooien van een quest moeten meerdere onafhankelijke acties plaatsvinden: XP toekennen, achievements checken, leaderboard bijwerken, en real-time broadcast sturen. Deze logica direct in de controller zetten maakt het ononderhoudbaar.

**Oplossing:** `QuestEventPublisher` beheert een lijst subscribers en stuurt `QuestCompletedEvent` door naar alle geabonneerden. Nieuwe functionaliteit toevoegen = nieuwe subscriber maken en registreren.

**Bestanden:**
- `Patterns/Observer/IQuestEventSubscriber.cs` - interface
- `Patterns/Observer/IQuestEventPublisher.cs` - interface
- `Patterns/Observer/QuestEventPublisher.cs` - publisher met Subscribe/Notify
- `Patterns/Observer/QuestCompletedEvent.cs` - event data
- `Patterns/Observer/XpCalculatorSubscriber.cs` - berekent XP, Gold, Level
- `Patterns/Observer/AchievementCheckerSubscriber.cs` - evalueert achievement regels
- `Patterns/Observer/LeaderboardUpdaterSubscriber.cs` - update leaderboard scores
- `Patterns/Observer/SignalRBroadcasterSubscriber.cs` - real-time push naar clients

**Code (Publisher):**
```csharp
public class QuestEventPublisher : IQuestEventPublisher
{
    private readonly List<IQuestEventSubscriber> _subscribers = new();

    public void Subscribe(IQuestEventSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
    }

    public void Notify(QuestCompletedEvent evt)
    {
        foreach (var subscriber in _subscribers)
            subscriber.OnQuestCompleted(evt);
    }
}
```

**Registratie in Program.cs:**
```csharp
var publisher = app.Services.GetRequiredService<IQuestEventPublisher>();
publisher.Subscribe(app.Services.GetRequiredService<XpCalculatorSubscriber>());
publisher.Subscribe(app.Services.GetRequiredService<AchievementCheckerSubscriber>());
publisher.Subscribe(app.Services.GetRequiredService<LeaderboardUpdaterSubscriber>());
publisher.Subscribe(app.Services.GetRequiredService<SignalRBroadcasterSubscriber>());
```

---

### 6. Interpreter (Behavioral)

**Probleem:** Achievement-regels moeten flexibel configureerbaar zijn (bijv. "voltooi 10 quests met rating boven 4.5") zonder voor elke nieuwe achievement code te schrijven.

**Oplossing:** Een eigen Domain-Specific Language (DSL) waarmee regels als leesbare strings worden gedefinieerd. `AchievementRuleParser` parset deze naar een expression tree die geëvalueerd wordt tegen freelancer-statistieken.

**Bestanden:**
- `Patterns/Interpreter/IExpression.cs` - interface met `Interpret(context)`
- `Patterns/Interpreter/ComparisonExpression.cs` - terminal: `quests.completed >= 10`
- `Patterns/Interpreter/AndExpression.cs` - non-terminal: logische AND
- `Patterns/Interpreter/OrExpression.cs` - non-terminal: logische OR
- `Patterns/Interpreter/AchievementContext.cs` - context met freelancer stats
- `Patterns/Interpreter/AchievementRuleParser.cs` - parser (string → expression tree)

**Voorbeeld DSL-regels (uit seed data):**
```
"quests.completed >= 1"
"quests.completed >= 10"
"quests.completed >= 25 and rating.avg >= 4.0"
"level >= 10 or quests.completed >= 20"
"rating.avg >= 4.5 and quests.completed >= 5"
```

**Code (Parser):**
```csharp
public static class AchievementRuleParser
{
    public static IExpression Parse(string rule)
    {
        // 1. Split op " or " → elk deel is een OR-operand
        var orParts = rule.Split(" or ", StringSplitOptions.TrimEntries);

        // 2. Elk OR-deel kan " and " bevatten
        var orExpressions = orParts.Select(ParseAndGroup).ToArray();

        // 3. Bouw OR expression tree
        var result = orExpressions[0];
        for (var i = 1; i < orExpressions.Length; i++)
            result = new OrExpression(result, orExpressions[i]);

        return result;
    }
}
```

**Voorbeeld evaluatie:**
```
Regel: "quests.completed >= 25 and rating.avg >= 4.0"

Parsed naar:
  AndExpression(
    ComparisonExpression("quests.completed", ">=", 25),
    ComparisonExpression("rating.avg", ">=", 4.0)
  )

Context (Alice): { quests.completed: 15, rating.avg: 4.8 }
Resultaat: false (15 < 25)
```

**Gebruikt in:** `AchievementCheckerSubscriber.OnQuestCompleted()` - na elke quest completion worden alle regels geëvalueerd.

---

### 7. Bridge (Structural)

**Probleem:** Notificaties hebben twee dimensies: het type bericht (quest update, achievement unlock) en het kanaal (email, push, webhook). Zonder Bridge heb je 2 types x 3 kanalen = 6 aparte klassen nodig.

**Oplossing:** De abstractie (`Notification`) en de implementatie (`INotificationSender`) zijn ontkoppeld. Elk type bericht kan via elk kanaal verstuurd worden door een andere sender mee te geven.

**Bestanden:**
- `Patterns/Bridge/INotificationSender.cs` - implementatie-interface
- `Patterns/Bridge/Notification.cs` - abstracte klasse (abstractie)
- `Patterns/Bridge/QuestAlert.cs` - refined abstractie: quest notificatie
- `Patterns/Bridge/AchievementAlert.cs` - refined abstractie: achievement notificatie
- `Patterns/Bridge/EmailSender.cs` - concrete implementatie
- `Patterns/Bridge/PushNotificationSender.cs` - concrete implementatie
- `Patterns/Bridge/WebhookSender.cs` - concrete implementatie

**Code:**
```csharp
// Abstractie
public abstract class Notification
{
    protected readonly INotificationSender _sender;
    protected Notification(INotificationSender sender) { _sender = sender; }
    public abstract string Notify(string recipient);
}

// Refined abstractie
public class QuestAlert : Notification
{
    public override string Notify(string recipient)
    {
        return _sender.Send(recipient, $"Quest Update: {QuestTitle}",
            $"Quest '{QuestTitle}' status changed to: {QuestStatus}");
    }
}

// Implementatie
public class EmailSender : INotificationSender
{
    public string Channel => "Email";
    public string Send(string recipient, string subject, string body)
    {
        return $"[EMAIL] To: {recipient} | Subject: {subject} | Body: {body}";
    }
}
```

**Gebruikt in:** `AchievementCheckerSubscriber` maakt een `AchievementAlert` aan met een `EmailSender` bij ontgrendeling. `NotificationsController` biedt een test-endpoint om elke combinatie te proberen.

---

### 8. Flyweight (Structural)

**Probleem:** Meerdere freelancers hebben dezelfde skills en badges. Zonder Flyweight wordt voor elke freelancer een apart "C#" skill-object aangemaakt - verspilling van geheugen.

**Oplossing:** `SkillFactory` en `BadgeFactory` zijn singleton pools die objecten cachen via `ConcurrentDictionary`. Dezelfde skill of badge wordt slechts één keer aangemaakt en daarna gedeeld.

**Bestanden:**
- `Patterns/Flyweight/SkillFlyweight.cs` - immutable shared object
- `Patterns/Flyweight/SkillFactory.cs` - singleton pool
- `Patterns/Flyweight/BadgeFlyweight.cs` - immutable shared object
- `Patterns/Flyweight/BadgeFactory.cs` - singleton pool

**Code:**
```csharp
public sealed class SkillFactory
{
    private static readonly Lazy<SkillFactory> _instance = new(() => new SkillFactory());
    private readonly ConcurrentDictionary<string, SkillFlyweight> _pool = new();

    public static SkillFactory Instance => _instance.Value;

    public SkillFlyweight GetSkill(string name, string category = "General", string iconUrl = "")
    {
        // GetOrAdd: als "C#" al in de pool zit, retourneer datzelfde object
        return _pool.GetOrAdd(name, key => new SkillFlyweight(key, category, iconUrl));
    }
}
```

**Gebruikt in:** `Program.cs` seed data registreert 15 skills en 8 badges in de pools. `AchievementCheckerSubscriber` haalt badges uit de `BadgeFactory` pool. `/api/debug/flyweight-stats` endpoint toont pool statistieken.

---

### 9. Monitor (Concurrency)

**Probleem:** Twee gebruikers kunnen tegelijkertijd dezelfde quest accepteren. Zonder synchronisatie accepteren beide threads de quest (race condition op `quest.Status` check-then-act).

**Oplossing:** `QuestAcceptanceLock` houdt een `ConcurrentDictionary<Guid, SemaphoreSlim>` bij. Per quest-ID wordt een `SemaphoreSlim(1,1)` (mutex) aangemaakt. De tweede thread wacht tot de eerste klaar is. Andere quests worden niet geblokkeerd (fine-grained locking).

**Bestanden:**
- `Patterns/Concurrency/QuestAcceptanceLock.cs`

**Code:**
```csharp
public class QuestAcceptanceLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(Guid questId)
    {
        return _locks.GetOrAdd(questId, _ => new SemaphoreSlim(1, 1));
    }

    public T ExecuteWithLock<T>(Guid questId, Func<T> action)
    {
        var semaphore = GetLock(questId);
        semaphore.Wait();
        try { return action(); }
        finally { semaphore.Release(); }
    }
}
```

**Gebruikt in:** `QuestService.AcceptQuest()`:
```csharp
public QuestResponseDto? AcceptQuest(Guid questId, Guid freelancerId)
{
    return _acceptanceLock.ExecuteWithLock(questId, () =>
    {
        if (quest.Status != QuestStatus.Open) return null; // thread-safe check
        quest.Status = QuestStatus.InProgress;
        return MapToResponse(quest);
    });
}
```

---

### 10. Producer-Consumer (Concurrency)

**Probleem:** Na quest completion moet de Observer chain 4 subscribers uitvoeren (XP, achievements, leaderboard, broadcast). Dit synchroon doen op de HTTP request thread maakt de API traag.

**Oplossing:** Events worden in een bounded `Channel<T>` queue geplaatst (producer = HTTP thread) en op een achtergrond-thread verwerkt door een `BackgroundService` (consumer). De HTTP response keert direct terug.

**Bestanden:**
- `Patterns/Concurrency/IEventQueue.cs` - interface
- `Patterns/Concurrency/EventQueue.cs` - bounded Channel-based queue
- `Patterns/Concurrency/EventProcessorService.cs` - BackgroundService (consumer)

**Code (Producer - in QuestService.CompleteQuest):**
```csharp
var evt = new QuestCompletedEvent(quest, freelancer, finalGold, finalXp);
_eventQueue.EnqueueAsync(evt).AsTask().Wait(); // event in queue, HTTP returnt direct
```

**Code (Consumer - EventProcessorService):**
```csharp
public class EventProcessorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in _eventQueue.DequeueAllAsync(stoppingToken))
        {
            _publisher.Notify(evt); // triggert alle Observer subscribers
        }
    }
}
```

**Code (Queue - bounded Channel):**
```csharp
public class EventQueue<T> : IEventQueue<T>
{
    private readonly Channel<T> _channel;

    public EventQueue(int capacity = 100)
    {
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }
}
```

---

### 11. Command (Behavioral)

**Probleem:** Quest lifecycle-acties (accepteren, voltooien, verlaten) muteren direct de domein-objecten. Fouten zijn onomkeerbaar en er is geen audittrail van uitgevoerde acties.

**Oplossing:** Elke actie wordt ingekapseld als een `IQuestCommand`-object met een `Execute()` en `Undo()` methode. De `QuestCommandInvoker` voert commands uit en bewaart een history stack, zodat acties teruggedraaid kunnen worden.

**Bestanden:**
- `Patterns/Command/IQuestCommand.cs` - interface: `Execute()` + `Undo()`
- `Patterns/Command/AcceptQuestCommand.cs` - wijst freelancer toe, zet status op InProgress
- `Patterns/Command/CompleteQuestCommand.cs` - markeert quest voltooid, kent XP/Gold toe
- `Patterns/Command/AbandonQuestCommand.cs` - geeft quest terug naar Open status
- `Patterns/Command/QuestCommandInvoker.cs` - voert commands uit en beheert undo-stack

**Code:**
```csharp
public interface IQuestCommand
{
    string CommandName { get; }
    void Execute();
    void Undo();
}

// Voorbeeld: quest accepteren en daarna ongedaan maken
var invoker = new QuestCommandInvoker();
invoker.Execute(new AcceptQuestCommand(quest, freelancer));
// quest.Status == InProgress, quest.AssignedFreelancerId == freelancer.Id

invoker.Undo();
// quest.Status == Open, quest.AssignedFreelancerId == null
```

**CompleteQuestCommand bewaart volledige state voor Undo:**
```csharp
public void Execute()
{
    _previousXp = _freelancer.Xp;
    _previousGold = _freelancer.Gold;
    _previousQuestsCompleted = _freelancer.QuestsCompleted;

    _quest.Status = QuestStatus.Completed;
    _freelancer.Xp += _quest.BaseXp;
    _freelancer.Gold += _quest.BaseGold;
    _freelancer.QuestsCompleted++;
}

public void Undo()
{
    _quest.Status = _previousStatus;
    _freelancer.Xp = _previousXp;
    _freelancer.Gold = _previousGold;
    _freelancer.QuestsCompleted = _previousQuestsCompleted;
}
```

**Gebruikt in:** Unit tests demonstreren de volledige accept → complete → undo flow. De `QuestCommandInvoker` kan uitgebreid worden met een redo-stack of persistente auditlog.

---

## Cross-Pattern Integratie

Het meest complexe punt in het project is `AchievementCheckerSubscriber`, waar 4 patterns samenwerken:

```
Quest Complete
    → Producer-Consumer (event in queue)
        → Observer (subscriber wordt getriggerd)
            → Interpreter (DSL regels evalueren)
            → Flyweight (badge uit pool halen)
            → Bridge (notificatie versturen)
```

```csharp
public void OnQuestCompleted(QuestCompletedEvent evt)    // Observer
{
    var context = new AchievementContext(properties);

    foreach (var achievement in _dataStore.Achievements.Values)
    {
        var expression = AchievementRuleParser.Parse(achievement.DslRule);  // Interpreter
        var achieved = expression.Interpret(context);

        if (achieved && !freelancer.Badges.Contains(achievement.BadgeName))
        {
            var badge = _badgeFactory.GetBadge(achievement.BadgeName, ...);  // Flyweight
            freelancer.Badges.Add(badge.Name);

            var alert = new AchievementAlert(new EmailSender(), ...);        // Bridge
            alert.Notify(freelancer.Email);
        }
    }
}
```

---

## Overige Architectuur

### Custom Exception Hierarchy
`QuestBoardException` (basis) met specifieke subklassen:
- `QuestBoardNotFoundException` → HTTP 404
- `InvalidQuestOperationException` → HTTP 409

Bestanden: `Exceptions/QuestBoardException.cs`

### Global Exception Handling Middleware
Vangt alle onverwerkte exceptions op en retourneert gestructureerde JSON-responses met juiste HTTP-statuscodes. Voorkomt dat stack traces naar de client lekken.

Bestanden: `Middleware/GlobalExceptionHandlerMiddleware.cs`

### SignalR Real-Time Updates
`QuestBoardHub` op `/hub/questboard` stuurt events naar verbonden clients:
- `QuestCompleted` - quest voltooid met XP/Gold info
- `AchievementUnlocked` - nieuwe achievement ontgrendeld
- `LeaderboardUpdated` - scores gewijzigd
- `QuestAccepted` - quest aangenomen

Bestanden: `Hubs/QuestBoardHub.cs`, `Hubs/IQuestBoardClient.cs`

### Dependency Injection (Program.cs)
Alle patterns worden geregistreerd via de ASP.NET Core DI container:
```csharp
builder.Services.AddSingleton(GameConfigurationManager.Instance);          // Singleton
builder.Services.AddSingleton<IPricingStrategy, DynamicPricingStrategy>(); // Strategy
builder.Services.AddSingleton<INotificationSender, EmailSender>();         // Bridge
builder.Services.AddSingleton<IQuestEventPublisher, QuestEventPublisher>();// Observer
builder.Services.AddSingleton<QuestAcceptanceLock>();                      // Monitor
builder.Services.AddSingleton<IEventQueue<QuestCompletedEvent>>(...);      // Producer-Consumer
builder.Services.AddHostedService<EventProcessorService>();                // Consumer
builder.Services.AddSingleton(SkillFactory.Instance);                      // Flyweight
```

---

## API Endpoints

| Method | Route | Beschrijving |
|--------|-------|-------------|
| GET | `/api/quests` | Alle quests ophalen |
| POST | `/api/quests` | Nieuwe quest aanmaken |
| GET | `/api/quests/{id}` | Quest detail |
| POST | `/api/quests/{id}/accept?freelancerId=` | Quest accepteren |
| POST | `/api/quests/{id}/complete` | Quest voltooien |
| GET | `/api/quests/{id}/matches?strategy=` | Matchmaking |
| GET | `/api/freelancers/{id}` | Freelancer profiel |
| GET | `/api/leaderboard` | Leaderboard |
| GET | `/api/achievements` | Alle achievements |
| POST | `/api/achievements/evaluate/{freelancerId}` | Achievements evalueren |
| POST | `/api/notifications/test` | Test notificatie versturen |
| GET | `/api/debug/flyweight-stats` | Flyweight pool statistieken |

---

## Technologieen

- **ASP.NET Core** (.NET 10) - Web API framework
- **SignalR** - Real-time WebSocket communicatie
- **System.Threading.Channels** - Bounded async queue (Producer-Consumer)
- **ConcurrentDictionary / SemaphoreSlim** - Thread-safe concurrency
- **Dependency Injection** - Ingebouwde ASP.NET Core DI container
