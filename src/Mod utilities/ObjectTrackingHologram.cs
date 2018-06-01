using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Mod_utilities
{
    public class ObjectTrackingHologram : Hologram
    {
        private Transform Target;
        private Vector3 Offset;

        private ObjectTrackingHologram(string text, Vector3 worldPosition) : base(text, worldPosition)
        {
        }

        public ObjectTrackingHologram(string text, Transform trackTarget) : base(text, trackTarget.position)
        {
            this.Target = trackTarget;
        }

        public ObjectTrackingHologram(string text, Transform trackTarget, Vector3 offset) : base(text, trackTarget.position)
        {
            this.Target = trackTarget;
            this.Offset = offset;
        }

        internal override void Update()
        {
            this.WorldPosition = this.Target.position + this.Offset;
        }
    }
}
