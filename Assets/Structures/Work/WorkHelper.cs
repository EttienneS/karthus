using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Structures.Work
{
    public static class WorkHelper
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

        public static Type GetTypeFor(string name)
        {
            return AllWorkOrderTypes.First(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static List<Type> _allWorkOrderTypes;

        public static List<Type> AllWorkOrderTypes
        {
            get
            {
                if (_allWorkOrderTypes == null)
                {
                    _allWorkOrderTypes = GetAllTypes(typeof(WorkOrderBase));
                }
                return _allWorkOrderTypes;
            }
        }
    }
}