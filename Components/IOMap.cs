using System;
using System.Collections.Generic;
using System.Linq;

namespace PiTung.Components
{
    public enum CubeSide
    {
        Top,
        Front,
        Left,
        Back,
        Right
    }

    public enum SideType
    {
        None,
        Input,
        Output
    }

    public class IOMap
    {
        internal IDictionary<CubeSide, SideType> Sides = new Dictionary<CubeSide, SideType>();
        internal event EventHandler Changed;

        public int InputCount => Sides.Values.Count(o => o == SideType.Input);
        public int OutputCount => Sides.Values.Count(o => o == SideType.Output);

        public IOMap()
        {
            foreach (CubeSide item in Enum.GetValues(typeof(CubeSide)))
            {
                Sides[item] = SideType.None;
            }
        }

        public IOMap SetSide(CubeSide side, SideType what)
        {
            Sides[side] = what;
            Changed?.Invoke(this, EventArgs.Empty);

            return this;
        }
    }
}
