using Assets.ServiceLocator;
using Needs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Creature.Behaviour
{
    public static class BehaviourController
    {
        public static IBehaviour GetBehaviour(string behaviourName)
        {
            try
            {
                var type = Loc.GetFileController().AllBehaviourTypes.First(behaviour => behaviour.Name.Equals(behaviourName, StringComparison.OrdinalIgnoreCase));
                return Activator.CreateInstance(type, null) as IBehaviour;
            }
            catch
            {
                Debug.LogError($"Could not load behaviour {behaviourName}");
                throw;
            }
        }

        internal static List<NeedBase> GetNeedsFor(string behaviourName)
        {
            var needs = new List<NeedBase>();
            switch (behaviourName.ToLower())
            {
                case "person":
                    needs = new List<NeedBase>
                {
                    new Hunger(),
                    new Energy(),
                    new Comfort(),
                    new Hygiene(),
                    new Needs.Social()
                };
                    break;

                default:
                    needs = new List<NeedBase>
                {
                    new Hunger(),
                    new Energy(),
                };
                    break;
            }
            return needs;
        }
    }
}