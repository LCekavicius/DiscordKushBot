using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace KushBot
{
    public sealed class EnumHelperV2Singleton
    {
        public EnumHelperV2 Helper { get; private set; }
        private EnumHelperV2Singleton() { Helper = new(); }
        private static EnumHelperV2Singleton instance = null;
        public static EnumHelperV2Singleton Instance
        {
            get
            {
                if (instance == null)
                    instance = new EnumHelperV2Singleton();
                return instance;
            }
        }
    }

    public class EnumHelperV2
    {
        public string ToString<T>(T value)
        {
            FieldInfo field = value?.GetType()?.GetField(value?.ToString());
            if (field == null)
                return String.Empty;
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public T GetEnumByDescriptedValue<T>(string value, bool autoCorrect = false)
        {
            if (autoCorrect)
            {
                value = char.ToUpper(value[0]) + value.Substring(1).ToLower();
            }
            var enumValues = Enum.GetValues(typeof(T));
            foreach (var item in enumValues)
            {
                if (ToString(item).Equals(value))
                    return (T)item;
            }
            return default(T);
        }

        public IEnumerable<string> ToStringList(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType);
            foreach (var value in enumValues)
            {
                yield return ToString(value);
            }
        }

    }
}
