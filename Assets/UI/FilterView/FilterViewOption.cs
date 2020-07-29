using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.UI
{
    public class FilterViewOption
    {
        public FilterViewOption(string name, Sprite sprite, params string[] categories)
        {
            Name = name;
            Sprite = sprite;
            Categories = categories.ToList();
        }

        public List<string> Categories { get; set; }
        public string Name { get; set; }
        public Sprite Sprite { get; set; }
    }
}