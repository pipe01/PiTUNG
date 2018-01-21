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
        /// <summary>
        /// Graphical utilities.
        /// </summary>
        public static GraphicUtilities Graphics => _Graphics;

        private static InputUtilities _Input = new InputUtilities();
        /// <summary>
        /// Input-related utilities.
        /// </summary>
        public static InputUtilities Input => _Input;


        /// <summary>
        /// True if we are one the main menu.
        /// </summary>
        public static bool IsOnMainMenu { get; internal set; } = true;

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
        
        private static FieldInfo GetField(Type type, string fieldName, bool isPrivate)
        {
            if (isPrivate)
                return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            else
                return type.GetField(fieldName);
        }

        /// <summary>
        /// Sets <paramref name="fieldName"/>'s value in <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object that has the field we want to change.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The new value for the field.</param>
        /// <param name="isPrivate">True if the field is private.</param>
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

        /// <summary>
        /// Gets <paramref name="fieldName"/>'s value in <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="T">The field's <typeparamref name="T"/>.</typeparam>
        /// <param name="obj">The object that contains the field.</param>
        /// <param name="fieldName">The field's name.</param>
        /// <param name="isPrivate">True if the field's private.</param>
        /// <returns>The value of the field.</returns>
        public static T GetFieldValue<T>(object obj, string fieldName, bool isPrivate)
        {
            FieldInfo field = GetField(obj.GetType(), fieldName, isPrivate);

            return (T)field.GetValue(obj);
        }

        /// <summary>
        /// Executes <paramref name="onObject"/>.<paramref name="methodName"/>
        /// </summary>
        /// <param name="onObject">The object that contains the method.</param>
        /// <param name="methodName">The method's name.</param>
        /// <param name="isPrivate">True if the method's private.</param>
        /// <param name="parameters">The method's parameters.</param>
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
