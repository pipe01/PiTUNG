using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Mod_utilities
{
    /// <summary>
    /// A hologram is a string that will float on screen, tracking a position in the world.
    /// <para />
    /// Note that when tracking a game object, if said game object is destroyed, the hologram will set its <see cref="Visible"/> property to false, but you must take care of destroying the hologram.
    /// </summary>
    public class Hologram
    {
        /// <summary>
        /// The world position that the hologram is currently tracking.
        /// </summary>
        public Vector3 WorldPosition { get; private set; }

        /// <summary>
        /// The hologram's current position on screen.
        /// </summary>
        public Vector2 ScreenPosition { get; private set; }

        /// <summary>
        /// The game object that the hologram is currently tracking. May be null.
        /// </summary>
        public GameObject TargetObject { get; private set; }

        /// <summary>
        /// The hologram's text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Whether or not to draw the hologram.
        /// </summary>
        public bool Visible { get; set; } = true;
        
        /// <summary>
        /// The hologram's text color.
        /// </summary>
        public Color TextColor { get; set; } = Color.white;

        private bool IsTrackingGameObject;

        private Hologram(string text)
        {
            this.Text = text;

            HologramManager.ActiveHolograms.Add(this);
        }
        
        /// <summary>
        /// Creates a new hologram that tracks the point <paramref name="worldPosition"/>.
        /// </summary>
        /// <param name="text">The hologram's text.</param>
        /// <param name="worldPosition">The tracking point.</param>
        public Hologram(string text, Vector3 worldPosition) : this(text)
        {
            this.WorldPosition = worldPosition;
        }

        /// <summary>
        /// Creates a new hologram that tracks the object <paramref name="gameObject"/>
        /// </summary>
        /// <param name="text">The hologram's text.</param>
        /// <param name="gameObject"></param>
        public Hologram(string text, GameObject gameObject) : this(text)
        {
            this.TargetObject = gameObject;
            this.IsTrackingGameObject = true;
        }

        /// <summary>
        /// Destroys this hologram. When destroyed, it won't be rendered on screen ever again.
        /// </summary>
        public void Destroy()
        {
            if (HologramManager.ActiveHolograms.Contains(this))
                HologramManager.ActiveHolograms.Remove(this);

            this.WorldPosition = Vector3.zero;
            this.ScreenPosition = Vector2.zero;
            this.TargetObject = null;
        }

        private void UpdateScreenPosition()
        {
            this.ScreenPosition = FirstPersonInteraction.FirstPersonCamera.WorldToScreenPoint(this.WorldPosition);
        }

        internal void Update()
        {
            if (this.TargetObject != null)
            {
                this.WorldPosition = this.TargetObject.transform.position;
            }
            else if (this.IsTrackingGameObject)
            {
                //We were tracking a game object but now it's null, which means that the object was destroyed.
                this.Visible = false;
            }

            UpdateScreenPosition();
        }

        internal void Draw()
        {
            if (this.Visible)
            {
                ModUtilities.Graphics.DrawText(this.Text, this.ScreenPosition, this.TextColor, true);
            }
        }
    }
}
