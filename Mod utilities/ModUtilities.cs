using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace PiTung_Bootstrap
{
    public static class ModUtilities
    {
        /// <summary>
        /// Graphical utilities.
        /// </summary>
        public static GraphicUtilities Graphics { get; } = new GraphicUtilities();

        /// <summary>
        /// Input-related utilities.
        /// </summary>
        public static InputUtilities Input { get; } = new InputUtilities();

        private static readonly IDictionary<KeyValuePair<Type, string>, FieldInfo> FieldCache = new Dictionary<KeyValuePair<Type, string>, FieldInfo>();

        /// <summary>
        /// True if we are one the main menu.
        /// </summary>
        public static bool IsOnMainMenu { get; internal set; } = true;

        /// <summary>
        /// The dummy component, use this to perform tasks that are normally run inside a MonoBehavior,
        /// like coroutines.
        /// </summary>
        public static DummyComponent DummyComponent { get; internal set; }

        /// <summary>
        /// Writes a line to the "output_log.txt" file.
        /// </summary>
        /// <param name="line">The line to be written. May be formatted with {0}, etc.</param>
        /// <param name="args">The arguments.</param>
        public static void Log(string line, params object[] args) => MDebug.WriteLine(line, 0, args: args);

        /// <summary>
        /// Writes a line to the "output_log.txt" file.
        /// </summary>
        /// <param name="line">The line to be written.</param>
        public static void Log(string line) => MDebug.WriteLine(line, 0, new object());

        private static FieldInfo GetField(Type type, string fieldName)
        {
            var key = new KeyValuePair<Type, string>(type, fieldName);

            if (!FieldCache.ContainsKey(key))
            {
                var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                FieldCache[key] = field ??
                    throw new ArgumentException($"Field {fieldName} not found in object of type {type.Name}.");
            }

            return FieldCache[key];
        }

        /// <summary>
        /// Sets <paramref name="fieldName"/>'s value in <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object that has the field we want to change.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The new value for the field.</param>
        /// <param name="isPrivate">True if the field is private.</param>
        public static void SetFieldValue<T>(object obj, string fieldName, T value)
        {
            var field = GetField(obj.GetType(), fieldName);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                throw new ArgumentException($"Field '{fieldName}' not found in {obj.GetType().Name}.", nameof(fieldName));
            }
        }

        /// <summary>
        /// Gets <paramref name="fieldName"/>'s value in <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="T">The field's type.</typeparam>
        /// <param name="obj">The object that contains the field.</param>
        /// <param name="fieldName">The field's name.</param>
        /// <param name="isPrivate">True if the field's private.</param>
        /// <returns>The value of the field.</returns>
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            FieldInfo field = GetField(obj.GetType(), fieldName);

            return (T)field.GetValue(obj);
        }

        /// <summary>
        /// Gets the static field <paramref name="fieldName"/>'s value in <typeparamref name="TParent"/>.
        /// </summary>
        /// <typeparam name="TParent">The type that contains the field.</typeparam>
        /// <typeparam name="TField">The field's type.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        public static TField GetStaticFieldValue<TParent, TField>(string fieldName)
        {
            FieldInfo field = GetField(typeof(TParent), fieldName);

            return (TField)field.GetValue(null);
        }

        /// <summary>
        /// Executes <paramref name="onObject"/>.<paramref name="methodName"/>
        /// </summary>
        /// <param name="onObject">The object that contains the method.</param>
        /// <param name="methodName">The method's name.</param>
        /// <param name="parameters">The method's parameters.</param>
        public static void ExecuteMethod(object onObject, string methodName, params object[] parameters)
        {
            if (onObject == null) throw new ArgumentNullException(nameof(onObject));
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));

            Type type = onObject.GetType();

            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (method == null) throw new ArgumentException($"Method '{methodName}' not found in object of type '{type.Name}'.", nameof(methodName));

            method.Invoke(onObject, parameters);
        }

        /// <summary>
        /// Gets the object that the player possesses.
        /// </summary>
        public static GameObject PlayerObject => GameObject.Find("FPSController").gameObject;
    }
}
