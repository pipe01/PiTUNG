using System.Data;
using PiTung_Bootstrap.Console;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    /// <summary>
    /// Represents a board in the TUNG world.
    /// </summary>
    public class Board
    {
        internal static int IdCounter = 1;

        /// <summary>
        /// The board's grid width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The board's grid height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The board's physical gameobject.
        /// </summary>
        public GameObject Object { get; }

        /// <summary>
        /// The board's unique ID. This ID will be unique inside a world.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The board's rotation, relative to its parent.
        /// </summary>
        public Quaternion Rotation { get; }

        /// <summary>
        /// Instantiate a new empty <see cref="Board"/> object.
        /// </summary>
        public Board() { }

        /// <summary>
        /// Instantiate a new <see cref="Board"/> object.
        /// </summary>
        /// <param name="w">The board's grid width.</param>
        /// <param name="h">The board's grid height.</param>
        /// <param name="obj">The board's game object.</param>
        /// <param name="id">The board's ID. If null, a unique ID will be automatically generated.</param>
        public Board(int w, int h, GameObject obj, int? id = null)
        {
            this.Width = w;
            this.Height = h;
            this.Object = obj;
            this.Rotation = obj.transform.localRotation;
            
            if (id != null)
            {
                this.Id = id.Value;

                if (IdCounter <= this.Id)
                {
                    IdCounter = this.Id + 1;
                }
            }
            else
            {
                this.Id = IdCounter++;
            }
        }

        /// <summary>
        /// Gets a circuit component at <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The component's X location.</param>
        /// <param name="y">The component's Y location.</param>
        /// <returns>The component's game object.</returns>
        public GameObject GetComponentAt(int x, int y, int side = 0)
        {
            float ay = (side == 0 ? 1 : -1) * .26f;
            Vector3 point = new Vector3(x + 0.5f, ay, y + 0.5f) * 0.3f;
            point = Object.transform.TransformPoint(point);
            
            foreach (var item in Physics.OverlapSphere(point, 0.1f))
            {
                if (item.transform.parent == Object.transform)
                    return item.gameObject;
            }
            
            return null;
        }

        /// <summary>
        /// Gets all the board's components.
        /// </summary>
        /// <returns>Key-value pairs containing the component's position and its game object.</returns>
        public IEnumerable<KeyValuePair<Vector2Int, GameObject>> GetComponents()
        {
            foreach (var item in Object.GetComponentsInChildren<Transform>())
            {
                if (item.parent != Object.transform)
                    continue;

                var obj = item.gameObject;

                var ax = Mathf.RoundToInt((obj.transform.localPosition.x - 0.5f) / 0.3f) + 1;
                var ay = Mathf.RoundToInt((obj.transform.localPosition.z - 0.5f) / 0.3f) + 1;

                yield return new KeyValuePair<Vector2Int, GameObject>(new Vector2Int(ax, ay), obj);
            }
        }
    }
}
