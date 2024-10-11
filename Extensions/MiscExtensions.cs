using System;
using System.Collections.Generic;
using System.Reflection;

namespace KushBot;

public static class MiscExtensions
{
    // ???? just make a UserBuffs class and put that there wtf
    public static Dictionary<ulong, int> LowerAndTryRemove(this Dictionary<ulong, int> dictionary, ulong userId)
    {
        if(dictionary.ContainsKey(userId))
        {
            dictionary[userId] -= 1;
        }

        if (dictionary[userId] <= 0)
        {
            dictionary.Remove(userId);
        }

        return dictionary;
    }

    public static object CloneObject(this object objSource)
    {
        Type typeSource = objSource.GetType();
        object objTarget = Activator.CreateInstance(typeSource);

        PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (PropertyInfo property in propertyInfo)
        {
            if (property.CanWrite)
            {
                if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                {
                    property.SetValue(objTarget, property.GetValue(objSource, null), null);
                }
                else
                {
                    object objPropertyValue = property.GetValue(objSource, null);
                    if (objPropertyValue == null)
                    {
                        property.SetValue(objTarget, null, null);
                    }
                    else
                    {
                        property.SetValue(objTarget, objPropertyValue.CloneObject(), null);
                    }
                }
            }
        }
        return objTarget;
    }
}
