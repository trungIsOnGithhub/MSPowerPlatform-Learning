using PAS.Model;
using PAS.Model.Dto;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using PAS.Common.Configurations;
using PAS.Common.Constants;

namespace PAS.Services
{
    public class KudoException : Exception
    {
        public KudoException(string message) : base(message) { }
    }
    public class KudoService : IKudoService
    {
        private readonly IKudoRepository _kudoRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IApplicationContext _applicationContext;
        private readonly IEmailService _emailService;
        public KudoService(IKudoRepository kudoRepository, IUserRepository userRepository, IApplicationContext applicationContext, IEmailService emailService, IConfigurationRepository configurationRepository)
        {
            _kudoRepository = kudoRepository;
            _userRepository = userRepository;
            _applicationContext = applicationContext;
            _emailService = emailService;
            _configurationRepository = configurationRepository;
        }

        public void GiveKudo(KudoCard kudo)
        {
 
            if (_applicationContext.CurrentUser == null)
            {
                throw new KudoException("[Kudo] Invalid user sending kudo");
            }
            if (_userRepository.GetUserById(kudo.ToUserId) == null)
            {
                throw new KudoException("[Kudo] Invalid user receiving kudo");
            }

            kudo.FromUserId = _applicationContext.CurrentUser.Id;
            string kudoFromConfig = _configurationRepository.GetConfigurationByKey(ConfigurationKeys.KudosPerMonth).Value;
            int kudoPerMonthFromConfig = int.Parse(kudoFromConfig);

            if (_kudoRepository.CountKudosSentThisMonth(kudo.FromUserId) < kudoPerMonthFromConfig)
            {
                _kudoRepository.AddNewKudo(kudo);
                var receiver = _userRepository.GetUserById(kudo.ToUserId);
                _emailService.KudoGiveNotification(receiver, kudo.Message);
                _kudoRepository.UnitOfWork.SaveChanges();
            }
            else
            {
                throw new KudoException(String.Format("[Kudo] You have reached the limit of kudos per month. You could only give maximum {0} kudos", kudoPerMonthFromConfig));
            }
        }

        public KudoResponse<KudoFromUser> ListKudosSentToUser()
        {
            if (_applicationContext.CurrentUser == null)
            {
                throw new KudoException("[Kudo] Invalid user receiving list kudos");
            }

            var kudos = _kudoRepository.GetKudosThisMonth(_applicationContext.CurrentUser.Id);
                
            return new KudoResponse<KudoFromUser>
            {
                Data = kudos,
                Count = kudos.Count
            };
        }

        public ICollection<KudoSummary> ListTop5(int month)
        {
            return _kudoRepository.SummaryTop5KudosReceived(month);
        }

        public RemainingKudos GetRemainingKudos()
        {
            string kudoFromConfig = _configurationRepository.GetConfigurationByKey(ConfigurationKeys.KudosPerMonth).Value;
            int kudoPerMonthFromConfig = int.Parse(kudoFromConfig);
            int currentKudosSent = _kudoRepository.CountKudosSentThisMonth(_applicationContext.CurrentUser.Id);
            return new RemainingKudos
            {
                AvailableKudos = kudoPerMonthFromConfig - currentKudosSent,
                MaximumKudos = kudoPerMonthFromConfig
            };
        }
    }
}
