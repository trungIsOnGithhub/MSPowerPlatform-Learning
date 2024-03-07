
using System.Collections.Generic;

namespace PAS.Services
{
    public interface ITemplateService
    {
        List<Model.Domain.Template> GetTemplates();
        List<Model.Domain.TemplateProfileCategory> GetTemplateHeadings(int templateId);
    }
}
