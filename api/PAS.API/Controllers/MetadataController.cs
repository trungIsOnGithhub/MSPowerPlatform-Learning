using PAS.Model.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using PAS.Model.Dto;
using PAS.Services;
using PAS.Model.Enum;
using PAS.Services;

namespace PAS.API.Controllers
{

    [RoutePrefix("api/metadata")]
    public class MetadataController : ApiController
    {
        private readonly IMetadataService _metadataService;
        private readonly IMetadataDtoMapper _metadataDtoMapper;
        public MetadataController(
            IMetadataService metadataService,
            IMetadataDtoMapper metadataDtoMapper
        )
        {
            _metadataService = metadataService;
            _metadataDtoMapper = metadataDtoMapper;
        }

        [HttpPost]
        [Route("saveChanges/{option}")]
        public MetadataEnum SaveChanges([FromBody] MetadataDtoBody body, [FromUri] MetadataEnum option)
        {
            try
            {
                if (body != null)
                {
                    _metadataService.SaveChanges(_metadataDtoMapper.ToDomains(body.UpdatedMetadata), body.DeletedMetadata, option);
                    return option;
                }

                return MetadataEnum.NoneMetadata;

            } catch (MetadataException)
            {
                return MetadataEnum.NoneMetadata;
            }
        }

        [HttpGet]
        [Route("getAll/{option}")]
        public IEnumerable<MetadataDtoBase> GetAll([FromUri] MetadataEnum option)
        {
            return _metadataService.GetAll(option);
        }

        [HttpGet]
        [Route("getAllActive/{option}")]
        public IEnumerable<MetadataDtoBase> GetAllActive([FromUri] MetadataEnum option)
        {
            return _metadataService.GetAllActive(option);
        }
    }
}