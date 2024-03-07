using PAS.Model.Mapping;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class CheckListService : ICheckListService
    {
        private ICheckListItemRepository checkListItemRepository;
        private ICheckListDtoMapper checkListDtoMapper;
        public CheckListService(ICheckListItemRepository checkListItemRepository, ICheckListDtoMapper checkListDtoMapper)
        {
            this.checkListItemRepository = checkListItemRepository;
            this.checkListDtoMapper = checkListDtoMapper;
        }
        public void CreateCheckList(List<Model.Dto.CheckListItem> checkList)
        {
            foreach (var item in checkList)
            {
                var domainObject = checkListDtoMapper.ToDomain(item);
                checkListItemRepository.CreateCheckListItem(domainObject);
            }
        }
    }
}
