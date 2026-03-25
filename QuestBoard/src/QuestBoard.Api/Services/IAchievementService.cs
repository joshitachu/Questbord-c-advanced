using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Models.DTOs;

namespace QuestBoard.Api.Services;

public interface IAchievementService
{
    List<Achievement> GetAllAchievements();
    Achievement CreateAchievement(CreateAchievementDto dto);
    object EvaluateForFreelancer(Guid freelancerId);
}
