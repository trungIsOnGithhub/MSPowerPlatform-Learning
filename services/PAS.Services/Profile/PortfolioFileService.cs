using PAS.Model.Domain;
using System;
using System.Collections.Generic;
using PAS.Repositories;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class PortfolioFileService : IPortfolioFileService
    {
        private readonly IPortfolioFileRepository portfolioFileRepository;
        public PortfolioFileService(IPortfolioFileRepository portfolioFileRepository)
        {
            this.portfolioFileRepository = portfolioFileRepository;
        }

        public Model.Domain.PortfolioFile AddPortfolioFile(PortfolioFile portfolioFile)
        {
            return portfolioFileRepository.AddPortfolioFile(portfolioFile);
        }

        public void DeletePortfolioFile(int id)
        {
            portfolioFileRepository.DeletePortfolioFile(id);
        }

        public List<PortfolioFile> GetPortfolioFiles(int profileItemId)
        {
            return portfolioFileRepository.GetPortfolioFiles(profileItemId);
        }

        public Model.Domain.PortfolioFile UpdatePortfolioFile(PortfolioFile portfolioFile)
        {
            return portfolioFileRepository.UpdatePortfolioFile(portfolioFile);
        }
    }
}
