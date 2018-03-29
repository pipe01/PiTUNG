using Harmony;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateCustomSavedObject")]
    internal class CreateCustomSavedObjectPatch
    {
        public static void Prefix(SavedCustomObject save, ObjectInfo CreateFromThis)
        {
            MDebug.WriteLine("SAVE CUSTOM OBJECT");

            /*
            CustomData structure:
            - CustomComponent.UniqueName
            - Output[0].On
            - Output[1].On
            - ...
            - Output[n].On
            */

            List<object> saveData = new List<object>();

            var customComponent = CreateFromThis.GetComponent<UpdateScript>().Component;

            saveData.Add(customComponent.UniqueName);
            
            CircuitOutput[] outputs = CreateFromThis.GetComponentsInChildren<CircuitOutput>();
            
            saveData.AddRange(outputs.Select(o => o.On).Cast<object>());

            save.CustomData = saveData.ToArray();

            MDebug.WriteLine("CUSTOMDATA:");
            foreach (var item in save.CustomData)
            {
                MDebug.WriteLine(item.ToString());
            }
        }
    }


    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateSavedObjectFrom", new Type[] { typeof(ObjectInfo) })]
    internal class CreateSavedObjectFromPatch
    {
        static bool Prefix(ref SavedObjectV2 __result, ObjectInfo worldsave)
        {
            SavedObjectV2 savedObjectV = null;

            if (worldsave.ComponentType == ComponentType.CustomObject)
            {
                savedObjectV = new SavedCustomObject();

                CreateCustomSavedObjectPatch.Prefix((SavedCustomObject)savedObjectV, worldsave);

                savedObjectV.LocalPosition = worldsave.transform.localPosition;
                savedObjectV.LocalEulerAngles = worldsave.transform.localEulerAngles;
                savedObjectV.Children = new SavedObjectV2[0];

                __result = savedObjectV;
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(SavedObjectUtilities), "GetCustomPrefab")]
    internal class GetCustomPrefabPatch
    {
        static void Postfix(ref GameObject __result,  SavedCustomObject save)
        {
            MDebug.WriteLine("LOAD CUSTOM COMPONENT 1");

            if (save.CustomData.Length == 0)
            {
                MDebug.WriteLine("ERROR: INVALID CUSTOM COMPONENT LOADED!");
                return;
            }

            string name = save.CustomData[0] as string;

            var prefab = ComponentRegistry.GetTypeFromName(name);

            if (prefab == null)
            {
                MDebug.WriteLine("ERROR: CUSTOM COMPONENT NOT FOUND!");
                return;
            }

            __result = ComponentRegistry.GetPrefabFromName(name);
        }
    }

    [HarmonyPatch(typeof(SavedObjectUtilities), "LoadCustomObject")]
    internal class LoadCustomObjectPatch
    {
        static void Prefix(GameObject LoadedObject, SavedCustomObject save)
        {
            MDebug.WriteLine("LOAD CUSTOM COMPONENT 2");

            CircuitOutput[] outputs = LoadedObject.GetComponentsInChildren<CircuitOutput>();

            int i = 0;
            foreach (var item in save.CustomData.Skip(1))
            {
                var output = outputs[i++];

                output.On = (bool)item;
            }

            foreach (var item in LoadedObject.GetComponents<UpdateScript>())
            {
                item.enabled = true;
            }
        }
    }
}
