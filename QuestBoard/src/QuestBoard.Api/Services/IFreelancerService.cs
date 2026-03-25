using QuestBoard.Api.Models.DTOs;

namespace QuestBoard.Api.Services;

public interface IFreelancerService
{
    FreelancerProfileDto CreateFreelancer(CreateFreelancerDto dto);
    FreelancerProfileDto? GetFreelancer(Guid id);
    FreelancerProfileDto? AddSkill(Guid freelancerId, string skillName);
}
