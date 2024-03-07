using PAS.Common.Constants;
using PAS.Model;
using PAS.Model.Dto;
using PAS.Model.Mapping;
using PAS.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;

namespace PAS.API.Controllers
{

    [RoutePrefix("api/kudos")]
    public class KudosController : ApiController
    {
        private readonly IKudoService kudoService;
        private readonly IKudoDtoMapper kudoMapper;

        public KudosController(IKudoService kudoService, IKudoDtoMapper kudoMapper)
        {
            this.kudoMapper = kudoMapper;
            this.kudoService = kudoService;
        }

        [HttpGet]
        public KudoResponse<KudoFromUser> GetKudosSentToUser()
        {
            try
            {
                var result = kudoService.ListKudosSentToUser();
                return result;
            }
            catch (KudoException)
            {
                return new KudoResponse<KudoFromUser>
                {
                    Data = new List<KudoFromUser>(),
                    Count = 0
                };
            }
        }

        [HttpPost]
        public IHttpActionResult PostNewKudo(KudoRequest kudoRequest)
        {
            try
            {
                if (kudoRequest == null || kudoRequest.Message == null)
                {
                    return BadRequest($"Invalid Input at POST request, error code {HttpStatusCode.BadRequest}");
                }

                kudoService.GiveKudo(kudoMapper.ToDomain(kudoRequest));

                return Ok();
            } catch (KudoException error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpGet]
        [Route("summary")]
        public ICollection<KudoSummary> GetKudoSumary(bool isLastMonth)
        {
            if (isLastMonth)
            {
                return kudoService.ListTop5(DateTime.Now.Month - 1);
            }
            else
            {
                return kudoService.ListTop5(DateTime.Now.Month);
            }
        }

        [HttpGet]
        [Route("remaining-kudos")]
        public RemainingKudos GetRemainingKudos()
        {
            return kudoService.GetRemainingKudos();
        }
    }
}
