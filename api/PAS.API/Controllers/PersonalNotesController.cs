using PAS.Model.Mapping;
using PAS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace PAS.API.Controllers
{
    [RoutePrefix("api/personalnotes")]
    public class PersonalNotesController : ApiController
    {
        private IPersonalNoteService _service;
        private IPersonalNoteDtoMapper _mapper;
        public PersonalNotesController(
            IPersonalNoteService service,
            IPersonalNoteDtoMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{storyId}")]
        public List<Model.Dto.PersonalNote> GetPersonalNotes(int storyId)
        {
            var personalNotes =  _service
                .GetPersonalNotes(storyId);
            return personalNotes;
        }

        [HttpPost]
        [Route("{userId}")]
        public Model.Dto.PersonalNote Create(Model.Dto.PersonalNote personalNote, int userId)
        {
            var result = _service.SavePersonalNote(personalNote, userId);
            return _mapper.ToDto(result);
        }

        [HttpPut]
        [Route("{id}")]
        public Model.Dto.PersonalNote Edit(
            int id,
            Model.Dto.PersonalNote personalNote)
        {
            var result =  _service.UpdatePersonalNotes(id, personalNote);
            return result;
        }

        [HttpDelete]
        [Route("{id}")]
        public bool DeletePersonalNote(int id)
        {
            _service.DeletePersonalNote(id);
            return true;
        }
    }
}
