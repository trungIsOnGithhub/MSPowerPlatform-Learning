using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAS.Common.Constants;
using PAS.Model;
using PAS.Model.Dto;

namespace PAS.Services
{
    public interface IKudoService
    {
        KudoResponse<KudoFromUser> ListKudosSentToUser();
        void GiveKudo(Model.KudoCard kudo);
        ICollection<KudoSummary> ListTop5(int month);
        RemainingKudos GetRemainingKudos();
    }
}
