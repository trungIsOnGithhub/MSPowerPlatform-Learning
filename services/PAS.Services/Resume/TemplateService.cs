
using PAS.Model.Domain;
using PAS.Repositories;
using System.Collections.Generic;

namespace PAS.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository templateRepository;

        public TemplateService(ITemplateRepository templateRepository)
        {
            this.templateRepository = templateRepository;
        }

        public List<TemplateProfileCategory> GetTemplateHeadings(int templateId)
        {
            return templateRepository.GetTemplateHeadings(templateId);
        }

        public List<Template> GetTemplates()
        {
            return templateRepository.GetTemplates();
        }
    }
}
