using System;
using System.Collections.Generic;
using PAS.Model.Domain;
using PAS.Model.Dto;
using PAS.Model.Enum;
using PAS.Model.Mapping;
using PAS.Repositories;


namespace PAS.Services
{
    public class MetadataException : Exception
    {
        public MetadataException(string message) : base(message) { }
    }
    public class MetadataService : IMetadataService
    {
        private readonly IMetadataRepository<Repositories.DataModel.Gender> _genderRepository;
        private readonly IMetadataRepository<Repositories.DataModel.Group> _groupRepository;
        private readonly IMetadataRepository<Repositories.DataModel.ResignationType> _resignationTypeRepository;
        private readonly IMetadataRepository<Repositories.DataModel.BillableType> _billableTypeRepository;
        private readonly IMetadataDtoMapper _metadataDtoMapper;
        public MetadataService(
            IMetadataDtoMapper metadataDtoMapper,
            IMetadataRepository<Repositories.DataModel.Gender> genderRepository,
            IMetadataRepository<Repositories.DataModel.Group> groupRepository,
            IMetadataRepository<Repositories.DataModel.ResignationType> resignationTypeRepository,
            IMetadataRepository<Repositories.DataModel.BillableType> billableTypeRepository)
        {
            _genderRepository = genderRepository;
            _groupRepository = groupRepository;
            _resignationTypeRepository = resignationTypeRepository;
            _billableTypeRepository = billableTypeRepository;
            _metadataDtoMapper = metadataDtoMapper;
        }

        public void SaveChanges(IEnumerable<MetadataBase> metadataDomains, IEnumerable<int> deletedIds, MetadataEnum option)
        {
            try
            {
                if (option == MetadataEnum.Gender)
                {
                    foreach (var id in deletedIds)
                    {
                        _genderRepository.Delete(id);
                    }

                    foreach (var metadata in metadataDomains)
                    {
                        if (metadata.Id == null)
                        {
                            _genderRepository.Add(metadata);
                        }
                        else
                        {
                            _genderRepository.Update(metadata);
                        }
                    }

                    _genderRepository.SaveEntities();
                }
                else if (option == MetadataEnum.Group)
                {
                    foreach (var id in deletedIds)
                    {
                        _groupRepository.Delete(id);
                    }

                    foreach (var metadata in metadataDomains)
                    {
                        if (metadata.Id == null)
                        {
                            _groupRepository.Add(metadata);
                        }
                        else
                        {
                            _groupRepository.Update(metadata);
                        }
                    }
                    _groupRepository.SaveEntities();
                }
                else if (option == MetadataEnum.ResignationType)
                {
                    foreach (var id in deletedIds)
                    {
                        _resignationTypeRepository.Delete(id);
                    }

                    foreach (var metadata in metadataDomains)
                    {
                        if (metadata.Id == null)
                        {
                            _resignationTypeRepository.Add(metadata);
                        }
                        else
                        {
                            _resignationTypeRepository.Update(metadata);
                        }
                    }
                    _resignationTypeRepository.SaveEntities();
                }
                else if (option == MetadataEnum.BillableType)
                {
                    foreach (var id in deletedIds)
                    {
                        _billableTypeRepository.Delete(id);
                    }

                    foreach (var metadata in metadataDomains)
                    {
                        if (metadata.Id == null)
                        {
                            _billableTypeRepository.Add(metadata);
                        }
                        else
                        {
                            _billableTypeRepository.Update(metadata);
                        }
                    }

                    _billableTypeRepository.SaveEntities();
                }
            } catch (MetadataRepoException error)
            {
                throw new MetadataException(error.Message);
            }
        }

        public IEnumerable<MetadataDtoBase> GetAll(MetadataEnum option)
        {
            if (option == MetadataEnum.Gender)
            {
                return _metadataDtoMapper.ToDtos(_genderRepository.GetAll());
            }
            else if (option == MetadataEnum.Group)
            {
                return _metadataDtoMapper.ToDtos(_groupRepository.GetAll());
            }
            else if (option == MetadataEnum.ResignationType)
            {
                return _metadataDtoMapper.ToDtos(_resignationTypeRepository.GetAll());
            }
            else if (option == MetadataEnum.BillableType)
            {
                return _metadataDtoMapper.ToDtos(_billableTypeRepository.GetAll());
            }
            return null;
        }
        public IEnumerable<MetadataDtoBase> GetAllActive(MetadataEnum option)
        {
            if (option == MetadataEnum.Gender)
            {
                return _metadataDtoMapper.ToDtos(_genderRepository.GetAllActive());
            }
            else if (option == MetadataEnum.Group)
            {
                return _metadataDtoMapper.ToDtos(_groupRepository.GetAllActive());
            }
            else if (option == MetadataEnum.ResignationType)
            {
                return _metadataDtoMapper.ToDtos(_resignationTypeRepository.GetAllActive());
            }
            else if (option == MetadataEnum.BillableType)
            {
                return _metadataDtoMapper.ToDtos(_billableTypeRepository.GetAllActive());
            }
            return null;
        }
    }
}
