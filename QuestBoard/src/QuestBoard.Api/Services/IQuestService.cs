using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Models.DTOs;

namespace QuestBoard.Api.Services;

public interface IQuestService
{
    QuestResponseDto CreateQuest(CreateQuestDto dto);
    List<QuestResponseDto> GetAllQuests();
    QuestResponseDto? GetQuest(Guid id);
    QuestResponseDto? AcceptQuest(Guid questId, Guid freelancerId);
    QuestResponseDto? CompleteQuest(Guid questId);
    List<(FreelancerProfileDto Freelancer, double Score)> GetMatches(Guid questId, string strategy = "SkillBased");
}
