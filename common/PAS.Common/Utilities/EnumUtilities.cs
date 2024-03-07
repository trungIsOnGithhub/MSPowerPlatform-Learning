using System;
using System.Collections.Generic;
using System.Linq;

namespace PAS.Common.Utilities
{
    public class Item
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
    public class EnumUtilities
    {
        public static List<T> GetAllValues<T>()
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToList();
        }
        public static List<Item> ToListItem(Type enumType)
        {
            List<Item> result = new List<Item>();
            foreach (int i in Enum.GetValues(enumType))
            {
                string name = Enum.GetName(enumType, i);
                Item item = new Item()
                {
                    Name = name.Replace("_", " "),
                    Value = i,
                };
                result.Add(item);
            }
            return result;
        }
    }
}
