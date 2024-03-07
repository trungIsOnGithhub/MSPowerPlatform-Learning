using PAS.Model.Domain;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class OptionListService : IOptionListService
    {
        private readonly ICriteriaOptionsListRepository _criteriaOptionsListRepository;
        private readonly IApplicationContext _context;

        public OptionListService(ICriteriaOptionsListRepository criteriaOptionsListRepository, IApplicationContext context)
        {
            _criteriaOptionsListRepository = criteriaOptionsListRepository;
            _context = context;
        }
        public bool CreateOptionList(Model.Domain.CriteriaOptionList listDto)
        {
            listDto.CreatedBy = _context.CurrentUser.Id;
            listDto.CreatedDate = DateTime.Now;
            if (listDto.Name.Equals(default(string)))
                throw new Exception("Invalid name");
            if (listDto.CriteriaOptions is null || listDto.CriteriaOptions.Count() == 0)
                throw new Exception("Invalid list item");
            listDto.CriteriaOptions = listDto.CriteriaOptions.Aggregate(
            new List<Model.Domain.CriteriaOptions>(), (newList, obj) =>
            {
                obj.CreatedDate = DateTime.Now;
                obj.CreatedBy = _context.CurrentUser.Id;
                newList.Add(obj);
                return newList;
            });
            _criteriaOptionsListRepository.Add(listDto);
            return _criteriaOptionsListRepository.UnitOfWork.SaveEntities();
        }

        public IEnumerable<CriteriaOptionList> GetOptionList()
        {
            return _criteriaOptionsListRepository.GetList();
        }
        public IEnumerable<CriteriaOptions> GetOptions()
        {
            return _criteriaOptionsListRepository.GetOptions().OrderBy(x=>x.SortOrder);
        }

        public bool RemoveOptionFromList(int optionListId, CriteriaOptions option)
        {
            _criteriaOptionsListRepository.DeleteOptionFromList(optionListId, option);
            return _criteriaOptionsListRepository.UnitOfWork.SaveEntities();
        }
        public bool AddOptionToList(int optionListId, CriteriaOptions option)
        {
            _criteriaOptionsListRepository.AddOptionToList(optionListId, option);
            return _criteriaOptionsListRepository.UnitOfWork.SaveEntities();
        }
        public CriteriaOptions UpdateOption(int optionId, CriteriaOptions option)
        {
            option.Id = optionId;
            _criteriaOptionsListRepository.UpdateOption(option);
            return _criteriaOptionsListRepository.UnitOfWork.SaveEntities() ? option : null;
        }
        public bool RemoveOptionList(int id)
        {
            var existedList = _criteriaOptionsListRepository.GetListByIdWithCriterion(id);
            if (existedList is null || !existedList.IsRemoveable)
                return false;
            _criteriaOptionsListRepository.RemoveList(id);
            return _criteriaOptionsListRepository.UnitOfWork.SaveEntities();
        }

        public bool UpdateList(CriteriaOptionList list)
        {
            if (list is null || list.Name == default(string) || list.CriteriaOptions.Count() == 0)
                return false;
            list.CriteriaOptions = list.CriteriaOptions.Aggregate(new List<CriteriaOptions>(), (newList, obj) =>
             {
                 obj.CreatedBy = this._context.CurrentUser.Id;
                 obj.CreatedDate = DateTime.Now;
                 newList.Add(obj);
                 return newList;
             });
            list.ModifiedBy = this._context.CurrentUser.Id;
            this._criteriaOptionsListRepository.UpdateOptionList(list);
            return this._criteriaOptionsListRepository.UnitOfWork.SaveEntities();
        }
    }
}
