using System;
using System.Collections.Generic;
using System.Linq;

namespace Structures.Work
{
    public static class WorkHelper
    {
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
                    _allWorkOrderTypes = ReflectionHelper.GetAllTypes(typeof(WorkOrderBase));
                }
                return _allWorkOrderTypes;
            }
        }
    }
}