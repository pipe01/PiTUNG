using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Mod_utilities
{
    /// <summary>
    /// A hologram that tracks an object in the world.
    /// </summary>
    public class ObjectTrackingHologram : Hologram
    {
        /// <summary>
        /// The hologram's position relative to the object's position.
        /// </summary>
        public Vector3 Offset { get; set; }

        private Transform Target;

        private ObjectTrackingHologram(string text, Vector3 worldPosition) : base(text, worldPosition)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="ObjectTrackingHologram"/>.
        /// </summary>
        /// <param name="text">The hologram text.</param>
        /// <param name="trackTarget">The object to track's transform.</param>
        public ObjectTrackingHologram(string text, Transform trackTarget) : base(text, trackTarget.position)
        {
            this.Target = trackTarget;
        }

        /// <summary>
        /// Instantiates a new <see cref="ObjectTrackingHologram"/>.
        /// </summary>
        /// <param name="text">The hologram text.</param>
        /// <param name="trackTarget">The object to track's transform.</param>
        /// <param name="offset">The hologram's position relative to the object's position.</param>
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
