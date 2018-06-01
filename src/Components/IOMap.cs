using System;
using System.Collections.Generic;
using System.Linq;

namespace PiTung.Components
{
    /// <summary>
    /// Represents a cube's sides
    /// </summary>
    public enum CubeSide
    {
        Top,
        Front,
        Left,
        Back,
        Right
    }

    /// <summary>
    /// Represents what a cube's side might be.
    /// </summary>
    public enum SideType
    {
        None,
        Input,
        Output
    }

    internal class IOMap
    {
        public struct IO
        {
            public CubeSide Side;
            public SideType Type;
            public float XOffset;
            public float YOffset;

            public IO(CubeSide side, SideType type, float xOffset, float yOffset)
            {
                this.Side = side;
                this.Type = type;
                this.XOffset = xOffset;
                this.YOffset = yOffset;
            }
        }

        public IList<IO> Sides = new List<IO>();
        public event EventHandler Changed;

        public int InputCount => Sides.Count(o => o.Type == SideType.Input);
        public int OutputCount => Sides.Count(o => o.Type == SideType.Output);
        
        public IOMap SetSide(CubeSide side, SideType what, float ox, float oy)
        {
            Sides.Add(new IO(side, what, ox, oy));
            Changed?.Invoke(this, EventArgs.Empty);

            return this;
        }
    }
}
