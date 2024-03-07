using System;
using System.Collections.Generic;
using PAS.Model.Dto;

namespace PAS.Services
{
    public interface IEnumService
    {
        List<Item> ToListItem(Type enumType);
    }
}
