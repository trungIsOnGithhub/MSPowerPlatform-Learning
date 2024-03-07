using System;
using System.Collections.Generic;
using System.Linq;
using PAS.Model.Domain;
using PAS.Model.Enum;
using PAS.Model.Dto;

namespace PAS.Services
{
    public interface IMetadataService
    {
        void SaveChanges(IEnumerable<MetadataBase> metadataDomains, IEnumerable<int> deletedIds, MetadataEnum option);
        IEnumerable<MetadataDtoBase> GetAll(MetadataEnum option);
        IEnumerable<MetadataDtoBase> GetAllActive(MetadataEnum option);
    }
}
