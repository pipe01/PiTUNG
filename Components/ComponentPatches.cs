using Harmony;
using PiTung.Console;
using References;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateCustomSavedObject")]
    internal static class CreateCustomSavedObjectPatch
    {
        public static void Prefix(SavedCustomObject save, ObjectInfo CreateFromThis)
        {
            /*
            *    CustomData structure:
            *    - CustomComponent.UniqueName
            *    - Outputs.On[]
            */

            List<object> saveData = new List<object>();

            var customComponent = CreateFromThis.GetComponent<UpdateHandler>();
            CircuitOutput[] outputs = CreateFromThis.GetComponentsInChildren<CircuitOutput>();

            if (customComponent.Component == null)
            {
                MDebug.WriteLine("!!customComponent.Component IS NULL!! NAME: " + customComponent.ComponentName);

                if (customComponent.ComponentName != null)
                {
                    customComponent.Component = ComponentRegistry.Registry[customComponent.ComponentName];
                }
                else
                {
                    MDebug.WriteLine("!!customComponent.ComponentName IS ALSO NULL!! ALLOW ME TO JUMP OFF A CLIFF");

                    return;
                }
            }
            
            saveData.Add(customComponent.Component.UniqueName);
            saveData.Add(outputs.Select(o => o.On).ToArray());

            save.CustomData = saveData.ToArray();
        }
    }
    
    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateSavedObjectFrom", new Type[] { typeof(ObjectInfo) })]
    internal static class CreateSavedObjectFromPatch
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
    internal static class GetCustomPrefabPatch
    {
        public static Transform NextParent = null;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            //Just replaces the method with "return null;", so that it doesn't get executed.

            yield return new CodeInstruction(OpCodes.Ldnull);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Postfix(ref GameObject __result,  SavedCustomObject save)
        {
            MDebug.WriteLine(save.CustomData == null);

            if (save.CustomData.Length == 0)
            {
                MDebug.WriteLine("ERROR: INVALID CUSTOM COMPONENT LOADED!");
                return;
            }

            string name = save.CustomData[0] as string;

            if (ComponentRegistry.Registry.TryGetValue(name, out var item))
            {
                __result = item.Instantiate();
                __result.transform.parent = NextParent;
            }
            else
            {
                MDebug.WriteLine("ERROR: CUSTOM COMPONENT NOT FOUND!");
            }
        }
    }

    [HarmonyPatch(typeof(SavedObjectUtilities), "LoadSavedObject")]
    internal static class LoadSavedObjectPatch
    {
        static void Prefix(SavedObjectV2 save, Transform parent)
        {
            GetCustomPrefabPatch.NextParent = parent;
        }
    }

    [HarmonyPatch(typeof(SavedObjectUtilities), "LoadCustomObject")]
    internal static class LoadCustomObjectPatch
    {
        static void Prefix(GameObject LoadedObject, SavedCustomObject save)
        {
            CircuitOutput[] outputs = LoadedObject.GetComponentsInChildren<CircuitOutput>();

            bool[] savedOutputs = (bool[])save.CustomData[1];

            if (outputs.Length != savedOutputs.Length)
            {
                MDebug.WriteLine("ERROR: INVALID CUSTOM COMPONENT DATA");
                return;
            }

            int i = 0;
            foreach (var item in savedOutputs)
            {
                outputs[i++].On = item;
            }
        }
    }


    [HarmonyPatch(typeof(ObjectInfo), "Start")]
    internal static class ObjectInfoStartPatch
    {
        static IList<GameObject> ObjectsWithObjectInfo = new List<GameObject>();

        static bool Prefix(ObjectInfo __instance)
        {
            if (ObjectsWithObjectInfo.Contains(__instance.gameObject))
            {
                GameObject.Destroy(__instance);
                return false;
            }

            ObjectsWithObjectInfo.Add(__instance.gameObject);

            return true;
        }
    }
    

    [HarmonyPatch(typeof(ComponentPlacer), "MakeSureThingBeingPlacedIsCorrect")]
    internal static class asdasd
    {
        static bool Prefix()
        {
            if (SelectionMenu.Instance.SelectedThing == SelectionMenu.Instance.PlaceableObjectTypes.Count)
            {
                if ((SelectionMenu.Instance.SelectedThingJustChanged || StuffPlacer.GetThingBeingPlaced == null) && ComponentRegistry.Registry.Count > 0)
                {
                    StuffPlacer.NewThingBeingPlaced(ComponentRegistry.Registry.Values.First().Instantiate());
                }

                return false;
            }
            
            return true;
        }
    }
}
