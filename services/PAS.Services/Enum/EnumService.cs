using System;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509.Qualified;
using PAS.Model.Dto;
using PAS.Model.Enum;

namespace PAS.Services
{
    public class EnumService: IEnumService
    {
        public List<Item> ToListItem(Type enumType)
        {
            List<Item> result = new List<Item>();
            foreach (int i in Enum.GetValues(enumType))
            {
                string name = Enum.GetName(enumType, i);
                Item item = new Item()
                {
                    Name = name.Replace("_"," "),
                    Value = i,
                };
                result.Add(item);
            }
            return result;
        }
    }
}
