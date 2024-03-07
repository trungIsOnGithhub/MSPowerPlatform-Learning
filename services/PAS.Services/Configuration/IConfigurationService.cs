using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Model.Domain;

namespace PAS.Services
{
    public interface IConfigurationService
    {
        string GetConfigurationByKey(string key);
        int GetIntConfigurationByKey(string key);
        void UpdateConfigurationByKey(Configuration configuration);
        void UpdateIntConfigurationByKey(Configuration configuration);
        List<Configuration> GetAllConfigurations();
    }
}
