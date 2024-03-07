using PAS.Model;
using PAS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PAS.Model.Dto;

namespace PAS.API.Controllers
{

    
    [RoutePrefix("api/careerPath")]
    public class CareerPathController : ApiController
    {
        private ICareerPathService careerPathService;
        public CareerPathController(ICareerPathService careerPathService)
        {
            this.careerPathService = careerPathService;
        }

        [HttpGet]
        [Route("storyId/{storyId}")]
        public List<StoryCareerPath> GetCareerPathByStoriesId(int storyId)
        {
            return careerPathService.GetCareerPathByStoryId(storyId);
        }

        [HttpGet]
        [Route("userId/{userId}")]
        public List<StoryCareerPath> GetStoryCareerPathCareerPathByUserId(int userId)
        {
            return careerPathService.GetStoryCareerPathByUserId(userId);
        }

        [HttpGet]
        [Route("{id}")]
        public StoryCareerPath GetStoryCareerPathById(int id)
        {
            return careerPathService.GetStoryCareerPathById(id);
        }

        [HttpGet]
        public List<StoryCareerPath> GetAllStoryCareerPath()
        {
            return careerPathService.GetAllStoryCareerPaths();
        }

        [HttpGet]
        [Route("templates")]
        public List<CareerPathTemplate> GetListCareerPathTemplate()
        {
            return careerPathService.GetAllPublicCareerPathTemplate();
        }
        [HttpGet]
        [Route("allTemplates")]
        public List<CareerPathTemplate> GetAllCareerPathTemplate()
        {
            return careerPathService.GetAllCareerPathTemplates();
        }

        [HttpGet]
        [Route("allTemplateSteps")]
        public List<CareerPathTemplateStep> GetAllCareerPathTemplateStep()
        {
            return careerPathService.getAllStoryCareerPathSteps();
        }

        [HttpGet]
        [Route("usageCounts")]
        public List<CareerPathTemplateUsageCount> GetCareerPathTemplateUsageCounts()
        {
            return careerPathService.GetCareerPathTemplateUsageCounts();
        }

        [HttpPost]
        [Route("template/create")]
        public CareerPathTemplate CreateCareerPathTemplate(CareerPathTemplate careerPathTemplate)
        {
            return careerPathService.CreateCareerPathTemplate(careerPathTemplate);
        }

        [HttpPost]
        [Route("template/update")]
        public CareerPathTemplate UpdateCareerPathTemplate(CareerPathTemplate careerPathTemplate)
        {
            return careerPathService.UpdateCareerPathTemplate(careerPathTemplate);
        }

        [HttpDelete]
        [Route("template/{id}")]
        public bool DeleteCareeerPathTemplate(int id)
        {
            return careerPathService.DeleteCareerPathTemplate(id);
        }

        [HttpPost]
        public StoryCareerPath CreateStoryCareerPath(StoryCareerPath careerPath)
        {
            return careerPathService.CreateCareerPath(careerPath);
        }

        [HttpPut]
        public StoryCareerPath UpdateStoryCareerPath(StoryCareerPath careerPath)
        {
            return careerPathService.UpdateCareerPath(careerPath);
        }

        [HttpPut]
        [Route("currentStep/{storyCareerPathId}")]
        public void UpdateCurrentStep(StoryCareerPathStep currentStep, int storyCareerPathId)
        {
            careerPathService.UpdateCurrentStep(currentStep, storyCareerPathId);
        }

        [HttpDelete]
        [Route("{id}")]
        public bool DeleteStoryCareerPath(int id)
        {
            careerPathService.DeleteCareerPath(id);
            return true;
        }

        [HttpPost]
        [Route("updateDate/{storyCareerPathStepId}")]
        public bool UpdateStoryCareerPathStep([FromBody] tempDates tempDates, [FromUri(Name = "storyCareerPathStepId")] int id)
        {
            DateTime startDate = tempDates.startDate;
            DateTime endDate = tempDates.endDate;
            return careerPathService.UpdateStoryCareerPathStep(id, startDate, endDate);
        }

        [HttpPut]
        [Route("technicalLevel/update/{userId}")]
        public bool UpdateTechnicalLevel(StoryCareerPath storyCareerPath, int userId)
        {
            return careerPathService.UpdateTechnicalLevel(storyCareerPath, userId);
        }

        [HttpPost]
        [Route("technicalLevel/assignMany/{userId}")]
        public IHttpActionResult AssignManyTechnicalLevel([FromBody] RequestAssignManyTechnicalLevel requestBody, int userId)
        {
            try
            {
                if (careerPathService.AssignManyTechnicalLevel(requestBody, userId)) return Ok(true);

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
