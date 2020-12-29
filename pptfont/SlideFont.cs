using System;
using System.Collections.Generic;
using System.Text;

namespace PPTXml
{
    public class L2Dictionary
    {
        Dictionary<string, HashSet<string>> _dict = new Dictionary<string, HashSet<string>>();

        public bool Add(string key, string level2Key)
        {
            bool exists = false;

            if (_dict.ContainsKey(key) == true)
            {
                exists = _dict[key].Contains(level2Key);

                if (exists == false)
                {
                    _dict[key].Add(level2Key);
                }
            }
            else
            {
                _dict[key] = new HashSet<string>() { level2Key };
            }

            return exists;
        }

        public L2Dictionary CreateNewMergeFrom(L2Dictionary other)
        {
            L2Dictionary newDict = new L2Dictionary();

            foreach (string item in _dict.Keys)
            {
                foreach (var sub in _dict[item])
                {
                    newDict.Add(item, sub);
                }
            }

            foreach (string item in other._dict.Keys)
            {
                foreach (var sub in other._dict[item])
                {
                    newDict.Add(item, sub);
                }
            }

            return newDict;
        }

        public IEnumerable<string> ListLevel2Key()
        {
            HashSet<string> values = new HashSet<string>();

            foreach (string item in _dict.Keys)
            {
                foreach (var sub in _dict[item])
                {
                    if (values.Contains(sub) == true)
                    {
                        continue;
                    }

                    values.Add(sub);
                    yield return sub;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string item in _dict.Keys)
            {
                sb.AppendLine(item);

                foreach (var sub in _dict[item])
                {
                    sb.AppendLine($"\t{sub}");
                }
            }

            return sb.ToString();
        }
    }
}
