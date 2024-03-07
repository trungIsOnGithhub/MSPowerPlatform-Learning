using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public interface IPortfolioFileService
    {
        List<Model.Domain.PortfolioFile> GetPortfolioFiles(int profileItemId);
        Model.Domain.PortfolioFile AddPortfolioFile(Model.Domain.PortfolioFile portfolioFile);
        void DeletePortfolioFile(int id);
        Model.Domain.PortfolioFile UpdatePortfolioFile(Model.Domain.PortfolioFile portfolioFile);
    }
}
