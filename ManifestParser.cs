using System.Data;
using System.CodeDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PiTung_Bootstrap.ModUpdater;
using static PiTung_Bootstrap.ModUpdater.Manifest;

namespace PiTung_Bootstrap
{
    public static class ManifestParser
    {
        public static Manifest ParseManifest(IEnumerable<string> lines)
        {
            var ret = new Manifest();
            var mods = new List<ModInfo>();
            ModInfo mod;
            int i = 0;

            foreach (var item in lines)
            {
                if (!string.IsNullOrEmpty(item.Trim()))
                {
                    if (item.StartsWith("["))
                        break;

                    if (!SetField(ret, item))
                        throw new ArgumentException($"Error at line {i}: invalid field.");
                }

                i++;
            }

            while ((mod = ParseMod(lines.ToArray(), ref i)) != null)
                mods.Add(mod);

            ret.Mods = mods.ToArray();

            return ret;
        }

        private static ModInfo ParseMod(string[] lines, ref int lineOffset)
        {
            ModInfo ret = null;

            int i;
            for (i = lineOffset; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line.Trim()))
                    continue;

                if (line.StartsWith("["))
                {
                    if (ret != null)
                        break;

                    if (!line.EndsWith("]"))
                        throw new ArgumentException($"Error at line {i}: missing ending bracket.");

                    ret = new ModInfo();
                    ret.Name = line.Substring(1, line.Length - 2);
                }
                else
                {
                    if (!SetField(ret, line))
                    {
                        throw new ArgumentException($"Error at line {i}: invalid field.");
                    }
                }
            }

            lineOffset = i;
            return ret;
        }

        private static bool SetField(object obj, string line)
        {
            if (!line.Contains('='))
            {
                return false;
            }

            int equalsIndex = line.IndexOf('=');
            string paramName = line.Substring(0, equalsIndex).Trim();
            string paramValue = line.Substring(equalsIndex + 1).Trim();

            return SetField(obj, paramName, paramValue);
        }

        private static bool SetField(object obj, string name, string strValue)
        {
            if (obj == null)
                return false;

            var field = obj.GetType().GetField(name);

            if (field == null)
                return false;

            object value = null;

            if (field.FieldType == typeof(string))
            {
                value = strValue;
            }
            else if (field.FieldType == typeof(int))
            {
                if (int.TryParse(strValue, out int num))
                {
                    value = num;
                }
                else
                {
                    return false;
                }
            }
            else if (field.FieldType == typeof(Version))
            {
                try
                {
                    value = new Version(strValue);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (value == null)
            {
                return false;
            }

            field.SetValue(obj, value);

            return true;
        }
    }
}
