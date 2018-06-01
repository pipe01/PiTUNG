using UnityEngine;

namespace PiTung.Mod_utilities
{
    /// <summary>
    /// A hologram is a string that will float on screen, tracking a position in the world. See <see cref="ObjectTrackingHologram"/>.
    /// </summary>
    public class Hologram
    {
        /// <summary>
        /// The world position that the hologram is currently tracking.
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// The hologram's current position on screen.
        /// </summary>
        public Vector2 ScreenPosition { get; private set; }
        
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
        public Color TextColor
        {
            get => this.Style.normal.textColor;
            set => this.Style.normal.textColor = value;
        }

        /// <summary>
        /// True if the hologram's text will have a shadow.
        /// </summary>
        public bool TextShadow { get; set; } = true;

        /// <summary>
        /// If true, the text will get bigger the closer you are to it, up to <see cref="MaxTextSize"/>.
        /// </summary>
        public bool ScaleSizeWithDistance { get; set; } = true;

        /// <summary>
        /// The maximum text size.
        /// </summary>
        public float MaxTextSize { get; set; } = 6f;

        private GUIStyle Style, ShadowStyle;

        #region ctor
        private Hologram(string text)
        {
            this.Text = text;
            this.Style = new GUIStyle
            {
                richText = true,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            };

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
        #endregion

        /// <summary>
        /// Destroys this hologram. When destroyed, it won't be rendered on screen ever again.
        /// </summary>
        public void Destroy()
        {
            if (HologramManager.ActiveHolograms.Contains(this))
                HologramManager.ActiveHolograms.Remove(this);

            this.WorldPosition = Vector3.zero;
            this.ScreenPosition = Vector2.zero;
        }

        private void UpdateScreenPosition()
        {
            var scrPos = FirstPersonInteraction.FirstPersonCamera.WorldToScreenPoint(this.WorldPosition);

            this.ScreenPosition = new Vector2(scrPos.x, Screen.height - scrPos.y);
        }

        private void UpdateShadowStyle()
        {
            this.ShadowStyle = new GUIStyle(this.Style);
            this.ShadowStyle.normal.textColor = Color.black;
        }

        private void CalcTextSize()
        {
            var camera = FirstPersonInteraction.FirstPersonCamera.transform.position;

            float distance = Vector3.Distance(camera, this.WorldPosition);
            
            int newSize = (int)(this.MaxTextSize * 2 * (1 / distance));

            if (newSize < 1)
                newSize = 1;
            else if (newSize < 0)
                newSize = 0;

            Style.fontSize = newSize;

            if (ShadowStyle != null)
                ShadowStyle.fontSize = newSize;
        }

        internal virtual void Update()
        {
            UpdateScreenPosition();
        }
        
        internal void Draw()
        {
            if (this.Visible)
            {
                if (this.ScaleSizeWithDistance)
                    CalcTextSize();
                else
                    Style.fontSize = (int)this.MaxTextSize;

                if (this.ShadowStyle == null)
                    UpdateShadowStyle();

                var size = this.Style.CalcSize(new GUIContent(this.Text));
                var pos = this.ScreenPosition;

                if (this.TextShadow)
                {
                    var shadowRect = new Rect(pos.x + 1, pos.y + 1, size.x, size.y);

                    GUI.Label(shadowRect, this.Text, this.ShadowStyle);
                }

                GUI.Label(new Rect(pos, size), this.Text, this.Style);
            }
        }
    }
}
