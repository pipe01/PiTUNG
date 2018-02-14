using System.Linq;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;

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
            MDebug.WriteLine($"{oldIndex} ---> {newIndex} ({list.Count} items)");
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
    }
}
