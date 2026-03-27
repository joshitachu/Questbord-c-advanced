namespace QuestBoard.Api.Exceptions;

// [EIGEN INBRENG: Custom Exception Hierarchy]
// Eigen exception-hierarchie voor gestructureerde foutafhandeling in het hele project.
// Elke exception heeft een ErrorCode waarmee de GlobalExceptionHandlerMiddleware
// automatisch de juiste HTTP-statuscode en foutmelding retourneert.

/// <summary>
/// Basis exception voor alle QuestBoard-specifieke fouten.
/// Bevat een ErrorCode voor gestructureerde foutafhandeling.
/// </summary>
public class QuestBoardException : Exception
{
    public string ErrorCode { get; }

    public QuestBoardException(string message, string errorCode = "QUESTBOARD_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public QuestBoardException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Gegooid wanneer een gevraagde entiteit niet bestaat.
/// Wordt door de middleware vertaald naar HTTP 404.
/// </summary>
public class QuestBoardNotFoundException : QuestBoardException
{
    public string EntityType { get; }
    public Guid EntityId { get; }

    public QuestBoardNotFoundException(string entityType, Guid entityId)
        : base($"{entityType} met ID '{entityId}' niet gevonden.", $"{entityType.ToUpper()}_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Gegooid bij ongeldige domein-operaties (bv. quest accepteren die al toegewezen is).
/// Wordt door de middleware vertaald naar HTTP 409 Conflict.
/// </summary>
public class InvalidQuestOperationException : QuestBoardException
{
    public InvalidQuestOperationException(string message)
        : base(message, "INVALID_OPERATION")
    {
    }

    public InvalidQuestOperationException(string message, Exception innerException)
        : base(message, "INVALID_OPERATION", innerException)
    {
    }
}
