using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung
{
    internal static class Extensions
    {
        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            var attrs = member.GetCustomAttributes(typeof(T), false);

            return attrs.Length == 0 ? null : (T)attrs[0];
        }

        public static bool EqualsVersion(this Version v1, Version v2)
        {
            bool comp(int a, int b) => a == -1 || b == -1 || a == b;

            bool ret = true;

            if (!comp(v1.Major, v2.Major))
                ret = false;

            if (ret && !comp(v1.Minor, v2.Minor))
                ret = false;

            if (ret && !comp(v1.Build, v2.Build))
                ret = false;

            if (ret && !comp(v1.Revision, v2.Revision))
                ret = false;

            return ret;
        }

        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);

            if (newIndex > oldIndex)
                newIndex--;

            list.Insert(newIndex, item);
        }

        public static bool TryGetValue<TItem, TKey>(this IList<TItem> list, TKey key, out TItem value, Func<TItem, TKey> selector)
        {
            var item = list.SingleOrDefault(o => Equals(key, selector(o)));

            if (!Equals(item, default(TItem)))
            {
                value = item;
                return true;
            }

            value = default(TItem);
            return false;
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();

            if (type != other.GetType())
                return null; // type mis-match

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

            foreach (var pinfo in type.GetProperties(flags))
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            foreach (var finfo in type.GetFields(flags))
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }

        public static string GetTextMeshProUGUIText(this GameObject obj)
        {
            foreach (var item in obj.GetComponentsInChildren<Component>())
            {
                var type = item.GetType();

                //Hacky way of setting the text to avoid importing the DLL
                if (type.Name == "TextMeshProUGUI")
                {
                    var str = (string)type.GetField("m_text", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item);

                    return str;
                }
            }

            return null;
        }
    }
}
