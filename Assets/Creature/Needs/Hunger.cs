using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Needs
{
    public class Hunger : NeedBase
    {
        public override string Icon { get; set; }

        public const string FeelingName = "Hunger";

        [JsonIgnore]
        public List<(string description, int impact, float threshold)> Levels = new List<(string description, int impact, float threshold)>
        {
            ("Ravenous",-20, 10),
            ("Hungry",-10, 30),
            ("Fine", 0, 80),
            ("Full", 5, 100),
        };


        public override void Update()
        {
            SetMoodFeeling(FeelingName, Levels);
        }
    }
}