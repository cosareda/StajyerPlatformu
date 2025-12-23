using StajyerPlatformu.Models;

namespace StajyerPlatformu.Services
{
    public interface IInternService
    {
        Task<InternProfile?> GetProfileAsync(string userId);
        Task<List<InternshipPost>> SearchJobsAsync(string? searchString, string? city, string? workType, string? companyName);
        Task<List<string>> GetCitiesAsync();
        Task<List<string>> GetCompaniesAsync();
        Task<InternProfile?> GetProfileForEditAsync(string userId);
        Task CreateOrUpdateProfileAsync(InternProfile model, string userId, string? profilePhotoPath, string? resumePath);
        Task<bool> AddExperienceAsync(Experience experience, string userId);
        Task<bool> DeleteExperienceAsync(int experienceId);
        Task<InternshipPost?> GetJobDetailAsync(int jobId);
        Task<bool> CheckIfAppliedAsync(int jobId, string userId);
        Task<bool> ApplyToJobAsync(int jobId, string userId);
        Task<List<Application>> GetMyApplicationsAsync(string userId);
    }
}

