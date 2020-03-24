using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class XCell
    {
        public static bool Equals<TKey, TValue>(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
        {
            // early-exit checks
            if (null == y)
                return null == x;
            if (null == x)
                return false;
            if (object.ReferenceEquals(x, y))
                return true;
            if (x.Count != y.Count)
                return false;

            // check keys are the same
            foreach (TKey k in x.Keys)
                if (!y.ContainsKey(k))
                    return false;

            // check values are the same
            foreach (TKey k in x.Keys)
            {
                var xE = x[k] as IEnumerable;
                var yE = y[k] as IEnumerable;
                var dxE = x[k] as IDictionary;
                var dyE = y[k] as IDictionary;
                if ((dxE as IDictionary) != null)
                {
                    foreach (var key in dxE.Keys)
                    {
                        if (!dyE.Contains(key)) return false;
                        if (!dxE[key].Equals(dyE[key])) return false;
                    }
                }
                else if ((xE as IList) != null)
                {
                    for (int i = 0; i < (xE as IList).Count; i++)
                    {
                        string str1 = (xE as IList)[i].ToString();
                        string str2 = (yE as IList)[i].ToString();
                        if (str1 != str2)
                            return false;
                    }
                }
                else
                {
                    if (!x[k].Equals(y[k]))
                        return false;
                }
            }

            return true;
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<int, string> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key.ToString()));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            sw.WriteLine("\t\t" + MakeValue(keyValue.Value));
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, int> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            sw.WriteLine("\t\t" + MakeValue(keyValue.Value.ToString()));
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, string[]> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (string s in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeValue(s));
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, string> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            sw.WriteLine("\t\t" + MakeValue(keyValue.Value));
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, List<int>> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (var value in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeValue(value.ToString()));
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, List<string>> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (var value in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeValue(value));
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, Dictionary<int, string>> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (var key2 in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeOpeningSubKey(key2.Key.ToString()));
                                sw.WriteLine("\t\t\t" + MakeValue(key2.Value.ToString()));
                                sw.WriteLine("\t\t" + MakeClosingSubKey());
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, Dictionary<string, string>> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (var key2 in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeOpeningSubKey(key2.Key));
                                sw.WriteLine("\t\t\t" + MakeValue(key2.Value));
                                sw.WriteLine("\t\t" + MakeClosingSubKey());
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void WriteDictionary(string dictName, string location, Dictionary<string, Dictionary<string, int>> dictionary)
        {
            if (File.Exists(location)) File.Delete(location);

            using (StreamWriter sw = File.AppendText(location))
            {
                sw.WriteLine(MakeOpeningTag(dictName));
                foreach (var key in dictionary.Keys)
                {
                    sw.WriteLine("\t" + MakeOpeningKey(key));
                    foreach (var keyValue in dictionary)
                    {
                        if (key == keyValue.Key)
                        {
                            foreach (var key2 in keyValue.Value)
                            {
                                sw.WriteLine("\t\t" + MakeOpeningSubKey(key2.Key));
                                sw.WriteLine("\t\t\t" + MakeValue(key2.Value.ToString()));
                                sw.WriteLine("\t\t" + MakeClosingSubKey());
                            }
                        }
                    }
                    sw.WriteLine("\t" + MakeClosingKey());
                }
                sw.WriteLine(MakeClosingTag(dictName));
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, Dictionary<int, string>> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, Dictionary<int, string>>();
                string key = "";
                string key2 = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name=") || line.Contains("<Subkey name="))
                    {
                        if (key == "")
                        {
                            key = ParseKey(line);
                            dict.Add(key, new Dictionary<int, string>());
                        }
                        else
                        {
                            key2 = ParseKey(line);
                            dict[key].Add(Convert.ToInt32(key2), "");
                        }
                    }
                    if (key != "" && key2 != "" && line.Contains("<value>"))
                    {
                        dict[key][Convert.ToInt32(key2)] = ParseValue(line);
                        key2 = "";
                    }
                    if (line.Contains("</key>"))
                    {
                        key = "";
                        key2 = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, Dictionary<string, string>> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, Dictionary<string, string>>();
                string key = "";
                string key2 = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name=") || line.Contains("<Subkey name="))
                    {
                        if (key == "")
                        {
                            key = ParseKey(line);
                            dict.Add(key, new Dictionary<string, string>());
                        }
                        else
                        {
                            key2 = ParseKey(line);
                            dict[key].Add(key2, "");
                        }
                    }
                    if (key != "" && key2 != "" && line.Contains("<value>"))
                    {
                        dict[key][key2] = ParseValue(line);
                        key2 = "";
                    }
                    if (line.Contains("</key>"))
                    {
                        key = "";
                        key2 = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, Dictionary<string, int>> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, Dictionary<string, int>>();
                string key = "";
                string key2 = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name=") || line.Contains("<Subkey name="))
                    {
                        if (key == "")
                        {
                            key = ParseKey(line);
                            dict.Add(key, new Dictionary<string, int>());
                        }
                        else
                        {
                            key2 = ParseKey(line);
                            dict[key].Add(key2, -1);
                        }
                    }
                    if (key != "" && key2 != "" && line.Contains("<value>"))
                    {
                        dict[key][key2] = Convert.ToInt32(ParseValue(line));
                        key2 = "";
                    }
                    if (line.Contains("</key>"))
                    {
                        key = "";
                        key2 = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<int, string> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<int, string>();
                string key = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(Convert.ToInt32(key), "");
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        dict[Convert.ToInt32(key)] = (ParseValue(line));
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, int> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, int>();
                string key = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(key, -1);
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        dict[key] = Convert.ToInt32(ParseValue(line));
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, string[]> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, string[]>();
                string key = "";
                List<string> values = new List<string>();
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(key, null);
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        values.Add(ParseValue(line));
                    }
                    if (line.Contains("</key>"))
                    {
                        dict[key] = values.ToArray();
                        values = new List<string>();
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, string> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, string>();
                string key = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(key, "");
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        dict[key] = (ParseValue(line));
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, List<int>> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, List<int>>();
                string key = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(key, new List<int>());
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        dict[key].Add(Convert.ToInt32(ParseValue(line)));
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static void ReadToDictionary(string location, ref Dictionary<string, List<string>> dict)
        {
            if (File.Exists(location))
            {
                dict = new Dictionary<string, List<string>>();
                string key = "";
                foreach (string line in File.ReadAllLines(location))
                {
                    if (line.Contains("<key name="))
                    {
                        key = ParseKey(line);
                        dict.Add(key, new List<string>());
                    }
                    if (key != "" && line.Contains("<value>"))
                    {
                        dict[key].Add(ParseValue(line));
                    }
                }
            }
            else
            {
                MessageBox.Show("File does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        public static string ParseKey(string line)
        {
            string l = line.Trim('\t');
            int firstIndex = l.IndexOf("=\"") + 2;
            int secondIndex = l.IndexOf("\">");
            return l.Substring(firstIndex, secondIndex - firstIndex);
        }

        public static string ParseValue(string line)
        {
            string l = line.Trim('\t');
            int firstIndex = l.IndexOf(">") + 1;
            int secondIndex = l.IndexOf("<", firstIndex);
            return l.Substring(firstIndex, secondIndex - firstIndex);
        }

        public static string MakeValue(string value)
        {
            return "<value>" + value + "</value>";
        }

        public static string MakeOpeningSubKey(string name)
        {
            return "<Subkey name=\"" + name + "\">";
        }

        public static string MakeClosingSubKey()
        {
            return "</Subkey>";
        }

        public static string MakeOpeningKey(string name)
        {
            return "<key name=\"" + name + "\">";
        }

        public static string MakeClosingKey()
        {
            return "</key>";
        }

        public static string MakeOpeningTag(string name)
        {
            return "<" + name + ">";
        }

        public static string MakeClosingTag(string name)
        {
            return "</" + name + ">";
        }
    }
}
