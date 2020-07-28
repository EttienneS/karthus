using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.UI
{
    public class FilterViewOption
    {
        private object _returnObject;
        public FilterViewOption(object returnObject, string name, Sprite sprite, params string[] categories)
        {
            _returnObject = returnObject;
            Name = name;
            Sprite = sprite;
            Categories = categories.ToList();
        }

        public List<string> Categories { get; set; }
        public string Name { get; set; }
        public Sprite Sprite { get; set; }
    }
}