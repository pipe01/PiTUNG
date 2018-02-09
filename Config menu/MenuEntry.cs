using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap.Config_menu
{
    /// <summary>
    /// Represents a menu entry in a config menu.
    /// </summary>
    public abstract class MenuEntry
    {
        /// <summary>
        /// This entry's parent entry. May be null if it's a root entry.
        /// </summary>
        public MenuEntry Parent { get; internal set; } = null;

        protected internal List<MenuEntry> Children { get; set; } = new List<MenuEntry>();

        /// <summary>
        /// Instantiates a new <see cref="MenuEntry"/> object.
        /// </summary>
        public MenuEntry()
        {
            //this.Children.ItemAdded += Children_ItemAdded;
        }

        private void Children_ItemAdded(MenuEntry item)
        {
            item.Parent = this;
        }

        /// <summary>
        /// Adds a child to this menu entry.
        /// </summary>
        /// <param name="child">The child to be added.</param>
        /// <returns>The same child.</returns>
        public MenuEntry AddChild(MenuEntry child)
        {
            this.Children.Add(child);
            return child;
        }

        /// <summary>
        /// Adds a new child of type <typeparamref name="T"/> to this menu entry.
        /// </summary>
        /// <typeparam name="T">The type of the menu entry to be added.</typeparam>
        /// <returns>The child that was added.</returns>
        public T AddChild<T>() where T : MenuEntry
        {
            var inst = Activator.CreateInstance<T>();
            AddChild(inst);
            return inst;
        }

        /// <summary>
        /// Adds children to this menu entry.
        /// </summary>
        /// <param name="children">A list containing the entries to be added.</param>
        public void AddChildren(params MenuEntry[] children)
        {
            foreach (var item in children)
            {
                AddChild(item);
            }
        }
    }

    public delegate void ValueChangedDelegate<T>(T value);
    public interface IValueMenuEntry<TValue>
    {
        /// <summary>
        /// Fired when this entry's value has been changed.
        /// </summary>
        event ValueChangedDelegate<TValue> ValueChanged;
    }

    /// <summary>
    /// Represents a menu entry that contains text.
    /// </summary>
    public class TextMenuEntry : MenuEntry
    {
        /// <summary>
        /// The entry's text.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Represents a menu entry whose function is to go up on the menu tree.
    /// </summary>
    internal sealed class GoUpMenuEntry : TextMenuEntry
    {
        public GoUpMenuEntry()
        {
            this.Text = "<i>Go back...</i>";
        }
    }

    /// <summary>
    /// Represents a menu entry that has two states: on and off.
    /// </summary>
    public sealed class CheckboxMenuEntry : TextMenuEntry, IValueMenuEntry<bool>
    {
        /// <summary>
        /// This entry's value.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// Fired when this entry's value has been changed.
        /// </summary>
        public event ValueChangedDelegate<bool> ValueChanged;

        /// <summary>
        /// Toggles the checkbox's value.
        /// </summary>
        public void Toggle()
        {
            Value = !Value;
            ValueChanged?.Invoke(Value);
        }
    }

    /// <summary>
    /// Represents a menu entry that has a numeric value.
    /// </summary>
    public sealed class SimpleNumberEntry : TextMenuEntry, IValueMenuEntry<float>
    {
        /// <summary>
        /// The entry's current value.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// The default step.
        /// </summary>
        public float Step { get; set; }

        /// <summary>
        /// The step to take when <see cref="BigStepKey"/> is down. Defaults to double of <see cref="Step"/>.
        /// </summary>
        public float BigStep { get; set; }

        /// <summary>
        /// The step to take when <see cref="SmallStepKey"/> is down. Defaults to half of <see cref="Step"/>.
        /// </summary>
        public float SmallStep { get; set; }

        /// <summary>
        /// The minimum value <see cref="Value"/> can reach.
        /// </summary>
        public float Minimum { get; set; }

        /// <summary>
        /// The maximum value <see cref="Value"/> can reach.
        /// </summary>
        public float Maximum { get; set; }
        
        /// <summary>
        /// Big step modifier key. See <see cref="BigStep"/>.
        /// </summary>
        public KeyCode BigStepKey { get; set; } = KeyCode.LeftShift;

        /// <summary>
        /// Small step modifier key. See <see cref="SmallStep"/>.
        /// </summary>
        public KeyCode SmallStepKey { get; set; } = KeyCode.LeftControl;

        /// <summary>
        /// Fired when the number has changed.
        /// </summary>
        public event ValueChangedDelegate<float> ValueChanged;

        private float GetStep()
        {
            if (Input.GetKey(this.BigStepKey))
                return this.BigStep;
            else if (Input.GetKey(this.SmallStepKey))
                return this.SmallStep;
            return this.Step;
        }

        /// <summary>
        /// Increments the value by one step.
        /// </summary>
        public void Increment()
        {
            float step = GetStep();

            if (this.Value == this.Maximum)
                return;

            if ((this.Value + step > this.Maximum) && (this.Value < this.Maximum))
                this.Value = this.Maximum;

            else if (this.Value < this.Maximum)
                this.Value += step;

            ValueChanged?.Invoke(this.Value);
        }

        /// <summary>
        /// Decrements the value by one step.
        /// </summary>
        public void Decrement()
        {
            float step = GetStep();

            if (this.Value == this.Minimum)
                return;

            if ((this.Value - step < this.Minimum) && (this.Value > this.Minimum))
                this.Value = this.Minimum;

            else if (this.Value > this.Minimum)
                this.Value -= step;

            ValueChanged?.Invoke(this.Value);
        }

        /// <summary>
        /// Instantiates a new <see cref="SimpleNumberEntry"/>.
        /// </summary>
        public SimpleNumberEntry(float step, float min, float max, float value = 0)
        {
            this.Value = value;
            this.Step = step;
            this.BigStep = step * 2;
            this.SmallStep = step / 2;
            this.Minimum = min;
            this.Maximum = max;
        }

        /// <summary>
        /// Instantiates a new <see cref="SimpleNumberEntry"/>.
        /// </summary>
        public SimpleNumberEntry(float step, float bigStep, float smallStep, float min, float max, float value = 0)
            : this(step, min, max, value)
        {
            this.BigStep = bigStep;
            this.SmallStep = smallStep;
        }
    }

    /// <summary>
    /// Represents a custom numeric value entry. Use this to get more control over the steps.
    /// </summary>
    public sealed class CustomNumericEntry : TextMenuEntry
    {
        public delegate void ChangeOneStepDelegate(ref float value);
        private ChangeOneStepDelegate IncrementMethod, DecrementMethod;

        private float _Value = 0;
        /// <summary>
        /// The entry's current value.
        /// </summary>
        public float Value => _Value;

        /// <summary>
        /// Increments the value by one step.
        /// </summary>
        public void Increment() => this.IncrementMethod(ref _Value);

        /// <summary>
        /// Decrements the value by one step.
        /// </summary>
        public void Decrement() => this.DecrementMethod(ref _Value);

        /// <summary>
        /// Instantiates a new <see cref="CustomNumericEntry"/> object with custom step delegates.
        /// </summary>
        /// <param name="increment">The method that will be called when an increment is requested.</param>
        /// <param name="decrement">The method that will be called when a decrement is required.</param>
        public CustomNumericEntry(ChangeOneStepDelegate increment, ChangeOneStepDelegate decrement)
        {
            this.IncrementMethod = increment;
            this.DecrementMethod = decrement;
        }

        /// <summary>
        /// Instantiates a new <see cref="CustomNumericEntry"/> object with default linear incrementation delegates.
        /// </summary>
        /// <param name="step">The size of the steps to take.</param>
        /// <param name="min">The maximum value.</param>
        /// <param name="max">The minimum value.</param>
        public CustomNumericEntry(float step, float min, float max)
        {
            this.IncrementMethod = (ref float o) => o += o < max ? step : 0;
            this.DecrementMethod = (ref float o) => o -= o > min ? step : 0;
        }
    }
}
