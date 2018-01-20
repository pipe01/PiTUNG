using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap
{
    public static class ModUtilities
    {
        private static GraphicUtilities _Graphics = new GraphicUtilities();
        public static GraphicUtilities Graphics => _Graphics;

        private static InputUtilities _Input = new InputUtilities();
        public static InputUtilities Input => _Input;



        public static bool IsOnMainMenu { get; internal set; } = true;

        public static void Log(string line, params object[] args) => MDebug.WriteLine(line, 0, args: args);
        public static void Log(string line) => MDebug.WriteLine(line, 0, new object());

        public static void DebugLog(string line, int level, params object[] args) => MDebug.WriteLine(line, level, args: args);
        public static void DebugLog(string line, int level) => MDebug.WriteLine(line, level, new object());

        private static FieldInfo GetField(Type type, string fieldName, bool isPrivate)
        {
            if (isPrivate)
                return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            else
                return type.GetField(fieldName);
        }

        public static void SetFieldValue<T>(object obj, string fieldName, T value, bool isPrivate)
        {
            var field = GetField(obj.GetType(), fieldName, isPrivate);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                throw new ArgumentException($"Field '{fieldName}' not found in {obj.GetType().Name}.", nameof(fieldName));
            }
        }

        public static T GetFieldValue<T>(object obj, string fieldName, bool isPrivate)
        {
            FieldInfo field = GetField(obj.GetType(), fieldName, isPrivate);

            return (T)field.GetValue(obj);
        }

        public static void ExecuteMethod(object onObject, string methodName, bool isPrivate, params object[] parameters)
        {
            if (onObject == null) throw new ArgumentNullException(nameof(onObject));
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));

            Type type = onObject.GetType();
            MethodInfo method;

            if (isPrivate)
                method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            else
                method = type.GetMethod(methodName);

            if (method == null) throw new ArgumentException(nameof(methodName));

            method.Invoke(onObject, parameters);
        }
    }
}
