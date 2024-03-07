using PAS.Model.Domain;
using PAS.Model.Dto;
using System.Collections.Generic;

namespace PAS.Services
{
    public interface IOptionListService
    {
        bool CreateOptionList(Model.Domain.CriteriaOptionList listDto);
        IEnumerable<Model.Domain.CriteriaOptionList> GetOptionList();
        IEnumerable<CriteriaOptions> GetOptions();
        bool AddOptionToList(int optionListId, CriteriaOptions option);
        bool RemoveOptionFromList(int optionListId, CriteriaOptions option);
        CriteriaOptions UpdateOption(int optionId, CriteriaOptions option);
        bool RemoveOptionList(int id);
        bool UpdateList(Model.Domain.CriteriaOptionList list);
    }
}