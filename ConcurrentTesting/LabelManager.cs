using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelManager
{
   public class LabelManager
   {
      private Dictionary<string, int> _nameDict;

      public LabelManager()
      {
         _nameDict = new Dictionary<string, int>();
      }

      private int NextIndex()
      {
         return _nameDict.Keys.Count;
      }

      public int Add(string label)
      {
         if (!(_nameDict.ContainsKey(label)))
         {
            _nameDict.Add(label, NextIndex());
         }
         return _nameDict[label];
      }

      public int? TryGetByName(string label)
      {
         if (_nameDict.ContainsKey(label))
         {
            return _nameDict[label];
         }
         {
            return null;
         }
      }

      public string TryGetById(int index)
      {
         var exists = _nameDict.Values.Contains(index);
         if (exists)
         {
            var item = _nameDict.First(entry => entry.Value == index);
            return item.Key;
         }
         else
         {
            return null;
         }
      }

      public IEnumerable<KeyValuePair<string, int>> Labels()
      {
         foreach (var entry in _nameDict)
         {
            yield return entry;
         }
      }
   }
}
