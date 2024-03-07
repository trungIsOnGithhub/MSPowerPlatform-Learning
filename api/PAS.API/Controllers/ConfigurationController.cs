using PAS.Model;
using PAS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PAS.Common.Constants;
using PAS.Model.Domain;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/configuration")]
    public class ConfigurationController: ApiController
    {
        private IConfigurationService configurationService;
        public ConfigurationController(IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        [HttpGet]
        [Route("{key}")]
        public string GetConfigurationByKey(string key)
        {
            return this.configurationService.GetConfigurationByKey(key);
        }

        [HttpGet]
        [Route("all")]
        public List<Configuration> GetAllConfigurations()
        {
            return this.configurationService.GetAllConfigurations();
        }

        [HttpGet]
        [Route("companyName")]
        public string GetCompanyName()
        {
            return this.configurationService.GetConfigurationByKey(ConfigurationKeys.CompanyName);
        }
        [HttpPut]
        [Route("update")]
        public void UpdateConfigurationByKey(Configuration configuration)
        {
            if (configuration == null)
                return;
            if (configuration.AlwaysUpdate)
            {
                this.configurationService.UpdateConfigurationByKey(configuration);
            }
        }
    }
}