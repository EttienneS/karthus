using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectionHelper
{
    public static List<Type> GetAllTypes(Type targetType)
    {
        var types = new List<Type>();
        if (types.Count == 0)
        {
            types.AddRange(Assembly.GetExecutingAssembly().GetTypes().Where(p => targetType.IsAssignableFrom(p)).ToList());
        }

        return types;
    }
}