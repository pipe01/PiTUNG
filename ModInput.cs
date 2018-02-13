using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung
{
    public class ModInput
    {
        public static ModInput Instance { get; } = new ModInput();

        private readonly string BindsPath = Application.persistentDataPath + "/bindings.ini";

        private Dictionary<string, KeyCode> Binds = new Dictionary<string, KeyCode>();

        private ModInput()
        {
        }

        #region Serialization
        private void LoadBinds()
        {
            Binds.Clear();

            if (!File.Exists(BindsPath))
            {
                File.WriteAllText(BindsPath, "");
                return;
            }

            string[] lines = File.ReadAllLines(BindsPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith(";") || line.Contains('='))
                    continue;

                int equalsIndex = line.IndexOf('=');
                string key = line.Substring(0, equalsIndex);
                string value = line.Substring(equalsIndex + 1);

                if (key.Contains(' '))
                    continue; //TODO: Log warning

                var keyObj = Enum.Parse(typeof(KeyCode), value, true);

                if (keyObj != null)
                {
                    Binds.Add(key, (KeyCode)keyObj);
                }
            }
        }
        private void SaveBinds()
        {
            StringBuilder str = new StringBuilder();

            foreach (var item in Binds)
            {
                str.AppendLine($"{item.Key}={Enum.GetName(typeof(KeyCode), item.Value)}");
            }

            File.WriteAllText(BindsPath, str.ToString());
        }
        #endregion
    }
}
