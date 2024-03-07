using PAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface ICareerPathService
    {
        List<StoryCareerPath> GetCareerPathByStoryId(int storyId);
        List<StoryCareerPath> GetStoryCareerPathByUserId(int userId);
        List<CareerPathTemplate> GetAllPublicCareerPathTemplate();
        List<StoryCareerPath> GetAllStoryCareerPaths();
        List<CareerPathTemplate> GetAllCareerPathTemplates();
        List<CareerPathTemplateUsageCount> GetCareerPathTemplateUsageCounts();
        CareerPathTemplate CreateCareerPathTemplate(CareerPathTemplate careerPathTemplate);
        CareerPathTemplate UpdateCareerPathTemplate(CareerPathTemplate careerPathTemplate);
        bool DeleteCareerPathTemplate(int id);
        StoryCareerPath GetStoryCareerPathById(int id);
        StoryCareerPath CreateCareerPath(StoryCareerPath careerPath);
        StoryCareerPath UpdateCareerPath(StoryCareerPath careerPath);
        void UpdateCurrentStep(StoryCareerPathStep currentStep, int storyCareerPathId);
        void DeleteCareerPath(int id);
        bool UpdateStoryCareerPathStep(int id, DateTime? startDate, DateTime? endDate);
        public bool UpdateTechnicalLevel(StoryCareerPath storyCareerPath, int userId);
        public List<CareerPathTemplateStep> getAllStoryCareerPathSteps();
        bool AssignManyTechnicalLevel(RequestAssignManyTechnicalLevel requestBody, int userId);
    }
}
