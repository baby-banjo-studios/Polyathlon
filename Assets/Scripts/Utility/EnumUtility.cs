using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BabyBanjo.Core.Utility
{
    public class EnumUtility
    {
        public static bool TryGetValueFromDescription<T>(string description, out T result)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                    {
                        result = (T)field.GetValue(null);
                        return true;
                    }
                }
                else
                {
                    if (field.Name == description)
                    {
                        result = (T)field.GetValue(null);
                        return true;
                    }
                }
            }
            result = default;
            return false;
        }

        public static bool TryGetDescriptionFromValue<T>(T value, out string result)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                result = attributes.First().Description;
                return true;
            }

            result = default;
            return false;
        }

        public static T NextValue<T>(T value) where T : Enum
        {
            T[] vals = (T[])Enum.GetValues(typeof(T));
            int idx = Array.IndexOf(vals, value) + 1;
            idx %= vals.Length;
            return vals[idx];
        }
    }
}