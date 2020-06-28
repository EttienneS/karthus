﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needs
{
    public class Social : NeedBase
    {
        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Lonely", -5, 0, 10),
        };

        public override string Icon { get; set; }

        [JsonIgnore]
        private float _chatDelta;

        public override void Update()
        {
            _chatDelta += Random.value;
            if (_chatDelta > 50 && Current < 80)
            {
                var friends = Creature.Faction.Creatures.Where(c => Creature.Awareness.Contains(c.Cell));
                if (friends.Any())
                {
                    _chatDelta = 0;
                    var friend = friends.GetRandomItem();

                    Creature.Say($"Hey {friend.Name}!");
                    friend.Say($"Sup {Creature.Name}!");

                    Creature.AddRelationshipEvent(friend, "Friendly Chat", 5);
                    Current += 25f;
                }
            }
        }

        public override string GetDescription()
        {
            return "The will to do more.";
        }
    }
}