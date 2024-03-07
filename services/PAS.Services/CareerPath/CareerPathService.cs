using PAS.Common;
using PAS.Model;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Repositories.Mapping;
using PAS.Model.Mapping;

namespace PAS.Services
{
    public class CareerPathService : ICareerPathService
    {
        private ICareerPathRepository careerPathRepository;
        private ICareerPathSecurityService securityService;
        private readonly IStoryService storyService;
        private IApplicationContext appContext;
        private Repositories.Mapping.IStoryMapper storyMapper;
        private IUserService userService;
        //private IStoryMapper storyDtoMapper;
        //private IStoryService storyService;

        public CareerPathService(
            ICareerPathRepository careerPathRepository,
            ICareerPathSecurityService securityService,
            IStoryService storyService,
            Repositories.Mapping.IStoryMapper storyMapper,
            IUserService userService,
            IApplicationContext appContext)
        {
            this.careerPathRepository = careerPathRepository;
            this.securityService = securityService;
            this.storyService = storyService;
            this.appContext = appContext;
            this.storyMapper = storyMapper;
            this.userService = userService;
            //this.storyDtoMapper = storyMapper;
            //this.storyService = storyService;
        }

        public StoryCareerPath GetStoryCareerPathById(int id)
        {
            var result = careerPathRepository.GetStoryCareerPathById(id);
            if (securityService.hasPermissionOnCareerPath(appContext.CurrentUser, result))
            {
                return result;
            }
            else
            {
                throw new UnauthorizedException("Don't have permission on this career path");
            }
        }

        public List<StoryCareerPath> GetCareerPathByStoryId(int id)
        {
            var result = careerPathRepository.GetStoryCareerPathByStoryId(id);
            foreach(var item in result)
            {
                if (!securityService.hasPermissionOnCareerPath(appContext.CurrentUser, item))
                {
                    throw new UnauthorizedException("Don't have permission on these career path");
                }
            }
            return result;
        }


        public List<StoryCareerPath> GetStoryCareerPathByUserId(int userId)
        {
            var result = careerPathRepository.GetStoryCareerPathByUserId(userId);
            foreach (var item in result)
            {
                if (!securityService.hasPermissionOnCareerPath(appContext.CurrentUser, item))
                {
                    throw new UnauthorizedException("Don't have permission on these career path");
                }
            }
            return result;
        }

        public List<CareerPathTemplate> GetAllPublicCareerPathTemplate()
        {
            return careerPathRepository.GetListCareerPathTemplate();
        }
        public List<CareerPathTemplate> GetAllCareerPathTemplates()
        {
                return careerPathRepository.GetAllCareerPathTemplates();
        }

        public List<CareerPathTemplateUsageCount> GetCareerPathTemplateUsageCounts()
        {
            return careerPathRepository.GetCareerPathTemplateUsageCounts();
        }

        public CareerPathTemplate CreateCareerPathTemplate(CareerPathTemplate careerPathTemplate)
        {
            return careerPathRepository.CreateCareerPathTemplate(careerPathTemplate);
        }

        public CareerPathTemplate UpdateCareerPathTemplate(CareerPathTemplate careerPathTemplate)
        {
            CareerPathTemplate result =  careerPathRepository.UpdateCareerPathTemplate(careerPathTemplate);

            // Check if new steps were added

            var newSteps = result.CareerPathTemplateSteps.Where(step => !careerPathTemplate.CareerPathTemplateSteps.Any(existingStep => existingStep.Id == step.Id));
            // Create StoryCareerPathStep for each StoryCareerPath
            foreach (var newStep in newSteps)
            {
                var storyCareerPaths = careerPathRepository.GetStoryCareerPathByCareerPathTemplateId(careerPathTemplate.Id);

                foreach (var storyCareerPath in storyCareerPaths)
                {
                    Model.StoryCareerPathStep storyCareerPathStep = new Model.StoryCareerPathStep();
                    storyCareerPathStep.StoryCareerPathId = (int) storyCareerPath.Id;
                    storyCareerPathStep.CareerPathTemplateStep.Id = newStep.Id;

                    careerPathRepository.CreateStoryCareerPathStep(storyCareerPathStep);
                }
            }
            return result;
        }

        public bool DeleteCareerPathTemplate(int id)
        {
            return careerPathRepository.DeleteCareerPathTemplate(id);
        }

        public List<StoryCareerPath> GetAllStoryCareerPaths()
        {
                return careerPathRepository.GetAllStoryCareerPaths(appContext.CurrentUser);
        }

        public StoryCareerPath CreateCareerPath(StoryCareerPath careerPath)
        {

            if (securityService.hasPermissionOnCareerPath(appContext.CurrentUser, careerPath))
            {
                return careerPathRepository.CreateCareerPath(careerPath);
            }
            else
            {
                throw new UnauthorizedException("Don't have permission on this career path");
            }

        }

        public StoryCareerPath UpdateCareerPath(StoryCareerPath careerPath)
        {
            if (securityService.hasPermissionOnCareerPath(appContext.CurrentUser, careerPath))
            {
                return careerPathRepository.UpdateCareerPath(careerPath, appContext.CurrentUser);
            }
            else
            {
                throw new UnauthorizedException("Don't have permission on this career path");
            }
        }

        public void DeleteCareerPath(int id)
        {
            var deletingCareerPath = careerPathRepository.GetStoryCareerPathById(id);
            if (securityService.hasPermissionOnCareerPath(appContext.CurrentUser, deletingCareerPath))
            {
                careerPathRepository.DeleteCareerPath(id);
            }
            else
            {
                throw new UnauthorizedException("Don't have permission on this career path");
            }
        }

        public void UpdateCurrentStep(StoryCareerPathStep currentStep, int storyCareerPathId)
        {
            var updatingCareerPath = GetStoryCareerPathById(storyCareerPathId);
            if (securityService.hasUpdateCurrentStepPermission(appContext.CurrentUser, updatingCareerPath))
            {
                careerPathRepository.UpdateCurrentStep(currentStep, storyCareerPathId, appContext.CurrentUser);
            }
            else
            {
                throw new UnauthorizedException("Don't have permission on updating this career path current step");
            }
        }
        public bool UpdateStoryCareerPathStep(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            this.careerPathRepository.UpdateStoryCareerPathStep(id, startDate, endDate);
            this.careerPathRepository.UnitOfWork.SaveEntities();
            return true;
        }

        public bool UpdateTechnicalLevel(StoryCareerPath storyCareerPath, int userId)
        {
            if (storyCareerPath.Id == 0)
            {
                PAS.Model.Story userStory = this.storyService.GetStoryLightByUserId(userId);
                storyCareerPath.Story = this.storyMapper.ToDto(userStory);
                StoryCareerPath newStoryCareerPath = CreateCareerPath(storyCareerPath);
                StoryCareerPathStep currentStep = newStoryCareerPath.StoryCareerPathSteps.Find(x => x.CareerPathTemplateStep.Id == storyCareerPath.CurrentCareerPathStep.CareerPathTemplateStep.Id);
                UpdateCurrentStep(currentStep, (int) newStoryCareerPath.Id);
                this.userService.UpdateUserTechLevel(userId, (int) newStoryCareerPath.Id);
                UpdateStoryCareerPathStep(currentStep.Id, storyCareerPath.StoryCareerPathSteps.Find(x => x.CareerPathTemplateStep.Id == storyCareerPath.CurrentCareerPathStep.CareerPathTemplateStep.Id).StartDate, null);
            }
            else
            {
                StoryCareerPath existedStoryCareerPath = GetStoryCareerPathById((int) storyCareerPath.Id);
                StoryCareerPathStep previousStep = existedStoryCareerPath.CurrentCareerPathStep;
                StoryCareerPathStep currentStep = existedStoryCareerPath.StoryCareerPathSteps.Find(x => x.CareerPathTemplateStep.Id == storyCareerPath.CurrentCareerPathStep.CareerPathTemplateStep.Id);
                UpdateCurrentStep(currentStep, (int) existedStoryCareerPath.Id);
                this.userService.UpdateUserTechLevel(userId, (int) existedStoryCareerPath.Id);
                UpdateStoryCareerPathStep(currentStep.Id, storyCareerPath.StoryCareerPathSteps.Find(x => x.CareerPathTemplateStep.Id == storyCareerPath.CurrentCareerPathStep.CareerPathTemplateStep.Id).StartDate, null);
            }
            return true;
        }

        public List<CareerPathTemplateStep> getAllStoryCareerPathSteps()
        {
            return this.careerPathRepository.GetAllStoryCareerPathSteps();
        }

        public bool AssignManyTechnicalLevel(RequestAssignManyTechnicalLevel requestBody, int userId)
        {
            // Assign Technical Level to the existed StoryCareerPath
            foreach (var updatedStoryCareerPath in requestBody.UpdatedTechnicalLevelStoryCareerPaths)
            {
                if (securityService.hasUpdateCurrentStepPermission(appContext.CurrentUser, updatedStoryCareerPath))
                {
                    // Update an existed StoryCareerPath
                    if (updatedStoryCareerPath.Id != null)
                    {
                        StoryCareerPath userStoryCareerPath = GetStoryCareerPathById((int) updatedStoryCareerPath.Id);
                        var currentPathStep = userStoryCareerPath.CurrentCareerPathStep;

                        if (currentPathStep != null)
                        {
                            // Check whether if the new assigned technical level is different from the current one
                            if (currentPathStep.Id != updatedStoryCareerPath.Id)
                            {
                                // Set EndDate = new StartDate of new technical level - 1 of the current Technical Level if it is null
                                if (userStoryCareerPath.CurrentCareerPathStep.EndDate == null)
                                {
                                    DateTime newStartDate = (DateTime)updatedStoryCareerPath.CurrentCareerPathStep.StartDate;
                                    UpdateStoryCareerPathStep(currentPathStep.Id, currentPathStep.StartDate, newStartDate.AddDays(-1));
                                }
                                // Assign Technical Level
                                UpdateCurrentStep(updatedStoryCareerPath.CurrentCareerPathStep, (int)updatedStoryCareerPath.Id);
                            }
                            // Update Start date and End date of Technical Level
                            UpdateStoryCareerPathStep(updatedStoryCareerPath.CurrentCareerPathStep.Id, updatedStoryCareerPath.CurrentCareerPathStep.StartDate, updatedStoryCareerPath.CurrentCareerPathStep.EndDate);
                        }
                        else
                        {
                            // If user currently doesn't have CurrentTechnicalLevel
                            UpdateCurrentStep(updatedStoryCareerPath.CurrentCareerPathStep, (int)updatedStoryCareerPath.Id);
                            UpdateStoryCareerPathStep(updatedStoryCareerPath.CurrentCareerPathStep.Id, updatedStoryCareerPath.CurrentCareerPathStep.StartDate, updatedStoryCareerPath.CurrentCareerPathStep.EndDate);
                        }
                    }
                    else
                    {
                        Model.Story userStory = storyService.GetStoryLightByUserId(userId);
                        updatedStoryCareerPath.Story = storyMapper.ToDto(userStory);
                        // Technical Level StartDate is already include in CurrentCareerPathStep
                        StoryCareerPath createdCareerPath = CreateCareerPath(updatedStoryCareerPath);
                        StoryCareerPathStep currentStep = createdCareerPath.StoryCareerPathSteps.Find(step => step.CareerPathTemplateStep.Id == updatedStoryCareerPath.CurrentCareerPathStep.CareerPathTemplateStep.Id);
                        UpdateCurrentStep(currentStep, (int)createdCareerPath.Id);

                        // Assign MainCareerPath for user if the created one is the selection from user
                        if (requestBody.MainStoryCareerPath != null)
                        {
                            if (requestBody.MainStoryCareerPath.Id == null && requestBody.MainStoryCareerPath.CareerPathTemplate.Id == createdCareerPath.CareerPathTemplate.Id)
                            {
                                userService.UpdateUserTechLevel(userId, (int)createdCareerPath.Id);
                            }
                        }
                    }
                }
                else
                {
                    throw new UnauthorizedException("Don't have permission on updating this career path current step");
                }
            }

            // Assign existed MainCareerPath for user
            if (requestBody.MainStoryCareerPath != null)
            {
                if (requestBody.MainStoryCareerPath.Id != null)
                {
                    userService.UpdateUserTechLevel(userId, (int)requestBody.MainStoryCareerPath.Id);
                }
            }

            foreach (int deletedId in requestBody.DeletedIdStoryCareerPaths)
            {
                DeleteCareerPath(deletedId);
            }

            return true;
        }
    }
}
