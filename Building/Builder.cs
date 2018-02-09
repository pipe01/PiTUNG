using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    /// <summary>
    /// Extensions for <see cref="Board"/>.
    /// </summary>
    public static class Builder
    {
        internal static List<KeyValuePair<CircuitInput, CircuitInput>> PendingIIConnections = new List<KeyValuePair<CircuitInput, CircuitInput>>();
        internal static List<KeyValuePair<CircuitInput, Output>> PendingIOConnections = new List<KeyValuePair<CircuitInput, Output>>();

        /// <summary>
        /// Adds a new <paramref name="component"/> to the board at <paramref name="x"/> and <paramref name="y"/> with <paramref name="rotation"/> rotation.
        /// </summary>
        /// <param name="board">This board.</param>
        /// <param name="component">The component to add.</param>
        /// <param name="x">The new component's X coordinate.</param>
        /// <param name="y">The new component's Y coordinate.</param>
        /// <param name="rotation">The new component's rotation. Defaults to 0.</param>
        /// <returns>True if the component was successfully added.</returns>
        public static bool AddCircuitComponent(this Board board, CircuitComponent component, int x, int y, float rotation = 0)
        {
            if (board.GetComponentAt(x, y) != null)
                return false;

            CircuitBoard circuit = board.Object.GetComponent<CircuitBoard>();

            GameObject gameObject = Object.Instantiate(component.Prefab, new Vector3(10000f, 10000f, 10000f), Quaternion.identity, circuit.transform);
            gameObject.transform.localPosition = new Vector3(x + 0.5f, .25f, y + 0.5f) * 0.3f;
            
            var num = (float)(Mathf.RoundToInt(gameObject.transform.localEulerAngles.y / 90f) * 90);
            num += rotation;

            gameObject.transform.localEulerAngles = new Vector3(0, num, 0f);
            gameObject.AddComponent<SaveThisObject>().ObjectType = component.Name;

            StuffPlacer.DestroyIntersectingConnections(gameObject);
            MegaMesh.AddMeshesFrom(gameObject);

            return true;
        }

        /// <summary>
        /// Tries to delete a component at <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="board">This board.</param>
        /// <param name="x">The component's X coordinate.</param>
        /// <param name="y">The component's Y coordinate.</param>
        /// <returns>True if the component was successfully deleted.</returns>
        public static bool DeleteCircuitComponent(this Board board, int x, int y)
        {
            var comp = board.GetComponentAt(x, y);

            if (comp != null)
            {
                StuffDeleter.DeleteThing(comp);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces the component at <paramref name="x"/> and <paramref name="y"/> with <paramref name="component"/>.
        /// </summary>
        /// <param name="board">This board.</param>
        /// <param name="x">The component's X coordinate.</param>
        /// <param name="y">The component's Y coordinate.</param>
        /// <param name="component">The new component.</param>
        /// <param name="rotation">The component's rotation.</param>
        public static void SetCircuitComponent(this Board board, int x, int y, CircuitComponent component, float rotation = 0)
        {
            board.DeleteCircuitComponent(x, y);
            board.AddCircuitComponent(component, x, y, rotation);
        }
        
        /// <summary>
        /// Connects A's input to B's output.
        /// </summary>
        /// <param name="board">This board.</param>
        /// <param name="inputX">A's X coordinate.</param>
        /// <param name="inputY">A's Y coordinate.</param>
        /// <param name="outputX">B's X coordinate.</param>
        /// <param name="outputY">B's Y coordinate.</param>
        /// <returns>True if the connection was successfully made.</returns>
        public static bool ConnectInputOutput(this Board board, int inputX, int inputY, int outputX, int outputY)
        {
            var input = GetComponentComponent<CircuitInput>(board, inputX, inputY);
            var output = GetComponentComponent<Output>(board, outputX, outputY);

            var kvp = new KeyValuePair<CircuitInput, Output>(input, output);

            PendingIOConnections.Add(kvp);

            StuffConnecter.CreateIOConnection(input, output);

            if (!PendingIOConnections.Contains(kvp))
            {
                return false;
            }
            else
            {
                PendingIOConnections.Remove(kvp);
                return true;
            }
        }

        /// <summary>
        /// Connects A's input to B's input.
        /// </summary>
        /// <param name="board">This board.</param>
        /// <param name="aX">A's X coordinate.</param>
        /// <param name="aY">A's Y coordinate.</param>
        /// <param name="bX">B's X coordinate.</param>
        /// <param name="bY">B's Y coordinate.</param>
        /// <returns>True if the connection was successfully made.</returns>
        public static bool ConnectInputInput(this Board board, int aX, int aY, int bX, int bY)
        {
            var a = GetComponentComponent<CircuitInput>(board, aX, aY);
            var b = GetComponentComponent<CircuitInput>(board, bX, bY);

            var kvp = new KeyValuePair<CircuitInput, CircuitInput>(a, b);

            PendingIIConnections.Add(kvp);

            StuffConnecter.CreateIIConnection(a, b);
            
            if (!PendingIIConnections.Contains(kvp))
            {
                return false;
            }
            else
            {
                PendingIIConnections.Remove(kvp);
                return true;
            }
        }

        private static TComponent GetComponentComponent<TComponent>(this Board board, int x, int y)
            where TComponent : MonoBehaviour
        {
            var aObj = board.GetComponentAt(x, y);
            
            if (aObj != null)
            {
                var input = aObj.GetComponentInChildren<TComponent>();

                if (input != null)
                {
                    return input;
                }
            }

            return null;
        }
    }
}
