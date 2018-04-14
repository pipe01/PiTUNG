using Harmony;
using PiTung.Console;
using PiTung.Mod_utilities;
using References;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            *    - Fields[]
            */

            List<object> saveData = new List<object>();

            var customComponent = CreateFromThis.GetComponent<UpdateHandler>();
            CircuitOutput[] outputs = CreateFromThis.GetComponentsInChildren<CircuitOutput>();

            if (customComponent.Component == null)
            {
                MDebug.WriteLine("!!customComponent.Component IS NULL!! NAME: " + customComponent.ComponentName);

                if (customComponent.ComponentName != null)
                {
                    if (ComponentRegistry.Registry.TryGetValue(customComponent.ComponentName, out var comp))
                    {
                        customComponent.Component = comp;
                    }
                    else
                    {
                        MDebug.WriteLine("!!MISING COMPONENT: " + customComponent.ComponentName);
                    }
                }
                else
                {
                    MDebug.WriteLine("!!customComponent.ComponentName IS ALSO NULL!! ALLOW ME TO JUMP OFF A CLIFF");

                    return;
                }
            }
            
            saveData.Add(customComponent.ComponentName);
            saveData.Add(outputs.Select(o => o.On).ToArray());
            saveData.AddRange(GetSaveThisFields(customComponent));

            save.CustomData = saveData.ToArray();
        }

        static IEnumerable<object> GetSaveThisFields(UpdateHandler handler)
        {
            var type = handler.GetType();

            foreach (var item in GetFieldsAndProperties(handler))
            {
                yield return "::" + item.Key;
                yield return item.Value;
            }
        }

        static IEnumerable<KeyValuePair<string, object>> GetFieldsAndProperties(object obj)
        {
            Type t = obj.GetType();

            foreach (var item in t.GetFields())
            {
                if (item.GetAttribute<SaveThisAttribute>() != null)
                    yield return new KeyValuePair<string, object>(item.Name, item.GetValue(obj));
            }

            foreach (var item in t.GetProperties())
            {
                if (item.GetAttribute<SaveThisAttribute>() != null)
                    yield return new KeyValuePair<string, object>(item.Name, item.GetValue(obj, null));
            }
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
            yield return new CodeInstruction(OpCodes.Ldnull);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Postfix(ref GameObject __result,  SavedCustomObject save)
        {
            if (save.CustomData.Length == 0)
            {
                MDebug.WriteLine("ERROR: INVALID CUSTOM COMPONENT LOADED!");
                __result = GetDefaultObject("");
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
                __result = GetDefaultObject(name);
            }
        }

        static GameObject GetDefaultObject(string componentName)
        {
            var obj = GameObject.Instantiate(Prefabs.Label);
            obj.transform.parent = NextParent;
            obj.AddComponent<EmptyHandler>().ComponentName = componentName;
            obj.GetComponent<MegaMeshComponent>().MaterialType = MaterialType.CircuitBoard;
            obj.GetComponent<Renderer>().material.color = Color.red;

            foreach (var item in obj.GetComponentsInChildren<MonoBehaviour>())
            {
                var type = item.GetType();

                if (type.Name == "TextMeshPro")
                {
                    type.InvokeMember("SetText", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, item, new object[] { componentName });

                    type.GetProperty("color").SetValue(item, Color.gray, null);
                }
            }

            return obj;
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
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Prefix(GameObject LoadedObject, SavedCustomObject save)
        {
            var handler = LoadedObject.GetComponent<UpdateHandler>();
            var handlerType = handler.GetType();

            Action<object> setValue = null;

            foreach (var item in save.CustomData.Skip(2))
            {
                if (item is string str && str.StartsWith("::"))
                {
                    string name = str.Substring(2);

                    var field = handlerType.GetField(name);
                    if (field == null)
                    {
                        var prop = handlerType.GetProperty(name);

                        if (prop == null)
                        {
                            MDebug.WriteLine("ERROR: INVALID DATA FIELD!");
                        }
                        else
                        {
                            setValue = o => prop.SetValue(handler, Convert.ChangeType(o, prop.PropertyType), null);
                        }
                    }
                    else
                    {
                        setValue = o => field.SetValue(handler, o);
                    }
                }
                else
                {
                    setValue?.Invoke(item);
                }
            }


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
            CustomMenu.Instance.Update();

            if (SelectionMenu.Instance.SelectedThing == SelectionMenu.Instance.PlaceableObjectTypes.Count)
            {
                if (!CustomMenu.Instance.Visible && SelectionMenu.Instance.SelectedThingJustChanged)
                    CustomMenu.Instance.Visible = true;

                if ((SelectionMenu.Instance.SelectedThingJustChanged || StuffPlacer.GetThingBeingPlaced == null || CustomMenu.Instance.SelectionChanged) && ComponentRegistry.Registry.Count > 0)
                {
                    var item = ComponentRegistry.Registry.Values.ElementAt(CustomMenu.Instance.Selected);

                    StuffPlacer.NewThingBeingPlaced(item.Instantiate());
                }

                return false;
            }
            else
            {
                CustomMenu.Instance.Visible = false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(HorizontalScrollMenuWithSelection), "ScrollThroughMenu")]
    internal static class RunBuildMenuPatch
    {
        static bool Prefix()
        {
            return !CustomMenu.Instance.Visible || !Input.GetKey(KeyCode.LeftControl);
        }
    }


    [HarmonyPatch(typeof(DummyComponent), "OnGUI")]
    internal static class ComponentGUIPatch
    {
        private static GUIStyle Style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };

        private static bool ShouldShow = Configuration.PitungConfig.Get("ShowComponentNameOnHover", true);

        static void Postfix()
        {
            if (ModUtilities.IsOnMainMenu || !ShouldShow)
                return;

            if (Physics.Raycast(FirstPersonInteraction.Ray(), out var hit, 20))
            {
                var info = hit.transform.GetComponent<ObjectInfo>() ??
                           hit.transform.parent?.GetComponent<ObjectInfo>();

                if (info != null && info.ComponentType == ComponentType.CustomObject)
                {
                    var handler = hit.transform.GetComponent<UpdateHandler>() ??
                                  hit.transform.GetComponentInChildren<UpdateHandler>();

                    if (handler != null)
                    {
                        string str = handler.Component?.DisplayName ?? handler.ComponentName;

                        var size = GUI.skin.box.CalcSize(new GUIContent(str));
                        size.x += 10;
                        size.y += 10;

                        var rect = new Rect(Screen.width / 2 - size.x / 2, 10, size.x, size.y);

                        GUI.Box(rect, str, Style);
                    }
                }
            }
        }
    }
}
