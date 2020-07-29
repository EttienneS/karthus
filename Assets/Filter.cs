using Assets.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    [Serializable]
    public class Filter
    {
        public List<string> _allowList = new List<string>();
        public List<string> _blockList = new List<string>();

        public bool Allows(ItemData item)
        {
            if (_blockList.Count == 0 && _allowList.Count == 0)
            {
                return true;
            }

            if (_blockList.Contains(item.Name) || _blockList.Intersect(item.Categories).Any())
            {
                return false;
            }

            return _allowList.Count == 0
                    || _allowList.Contains(item.Name)
                    || _allowList.Intersect(item.Categories).Any();
        }

        internal void Clear()
        {
            _allowList.Clear();
            _blockList.Clear();
        }

        internal void AddAllowedItem(string name)
        {
            _allowList.Add(name);
        }

        internal void AddBlockedItem(string name)
        {
            _blockList.Add(name);
        }

        public override string ToString()
        {
            if (_blockList.Count == 0 && _allowList.Count == 0)
            {
                return "Accepts anything";
            }
            else
            {
                var message = string.Empty;
                if (_allowList.Count > 0)
                {
                    message += "Accepts: ";
                    foreach (var item in _allowList)
                    {
                        message += item + ", ";
                    }
                    message = message.Trim().Trim(',') + "\n";
                }
                if (_blockList.Count > 0)
                {
                    message += "Does not accept: ";
                    foreach (var item in _blockList)
                    {
                        message += item + ", ";
                    }
                    message = message.Trim().Trim(',');
                }
                return message;
            }
        }
    }
}