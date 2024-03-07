using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Repositories;
using PAS.Model.Domain;

namespace PAS.Services
{
    public class ConfigurationService: IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        public ConfigurationService(IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }
        public string GetConfigurationByKey(string key)
        {
            return _configurationRepository.GetConfigurationByKey(key).Value;
        }
        public int GetIntConfigurationByKey(string key)
        {
            string configValue = GetConfigurationByKey(key);
            if (configValue != null)
            {
                if (int.TryParse(configValue, out int intValue))
                {
                    return intValue;
                }
                throw new ArgumentException($"Invalid integer value: {configValue}");
            }
            throw new ArgumentException($"Configuration not found for key: {key}");
        }
        public List<Configuration> GetAllConfigurations()
        {
            return _configurationRepository.GetAllConfigurations();
        }
        public void UpdateConfigurationByKey(Configuration configuration)
        {
            _configurationRepository.UpdateConfigurationByKey(configuration);
        }
        public void UpdateIntConfigurationByKey(Configuration configuration)
        {
            string stringValue = configuration.Value.ToString();
            _configurationRepository.UpdateConfigurationByKey(configuration);
        }
    }
}
