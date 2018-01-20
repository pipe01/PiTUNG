using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap.Config_menu
{
    public class MenuEntry
    {
        public string Text { get; set; }

        public List<MenuEntry> Children { get; set; } = new List<MenuEntry>();
    }

    public class CheckboxMenuEntry : MenuEntry
    {
        public bool Value { get; set; }

        public void Toggle() => Value = !Value;
    }

    public class SimpleNumberEntry : MenuEntry
    {
        public float Value { get; set; }
        public float Step { get; set; }
        public float BigStep { get; set; }
        public float SmallStep { get; set; }
        public float Minimum { get; set; }
        public float Maximum { get; set; }

        public KeyCode BigStepKey { get; set; } = KeyCode.LeftShift;
        public KeyCode SmallStepKey { get; set; } = KeyCode.LeftControl;

        private float GetStep()
        {
            if (Input.GetKey(this.BigStepKey))
                return this.BigStep;
            else if (Input.GetKey(this.SmallStepKey))
                return this.SmallStep;
            return this.Step;
        }

        public void Increment()
        {
            float step = GetStep();

            if ((this.Value + step > this.Maximum) && (this.Value < this.Maximum))
                this.Value = this.Maximum;

            else if (this.Value < this.Maximum)
                this.Value += step;
        }

        public void Decrement()
        {
            float step = GetStep();

            if ((this.Value - step < this.Minimum) && (this.Value > this.Minimum))
                this.Value = this.Minimum;

            else if (this.Value > this.Minimum)
                this.Value -= step;
        }

        public SimpleNumberEntry(float step, float min, float max, float value = 0)
        {
            this.Value = value;
            this.Step = step;
            this.BigStep = step * 2;
            this.SmallStep = step / 2;
            this.Minimum = min;
            this.Maximum = max;
        }

        public SimpleNumberEntry(float step, float bigStep, float smallStep, float min, float max, float value = 0)
            : this(step, min, max, value)
        {
            this.BigStep = bigStep;
            this.SmallStep = smallStep;
        }
    }

    public class NumericRangeEntry : MenuEntry
    {
        public delegate void ChangeOneStepDelegate(ref float value);
        private ChangeOneStepDelegate IncrementMethod, DecrementMethod;

        private float _Value = 0;
        public float Value => _Value;
        
        public void Increment() => this.IncrementMethod(ref _Value);

        public void Decrement() => this.DecrementMethod(ref _Value);

        public NumericRangeEntry(float step, float min, float max)
        {
            this.IncrementMethod = (ref float o) => o += o < max ? step : 0;
            this.DecrementMethod = (ref float o) => o -= o > min ? step : 0;
        }
        public NumericRangeEntry(ChangeOneStepDelegate increment, ChangeOneStepDelegate decrement)
        {
            this.IncrementMethod = increment;
            this.DecrementMethod = decrement;
        }
    }
}
