using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    public static class Builder
    {
        internal static List<KeyValuePair<CircuitInput, CircuitInput>> PendingIIConnections = new List<KeyValuePair<CircuitInput, CircuitInput>>();
        internal static List<KeyValuePair<CircuitInput, Output>> PendingIOConnections = new List<KeyValuePair<CircuitInput, Output>>();

        public static bool AddBoardComponent(this Board board, CircuitComponent component, int x, int y, float rotation = 0)
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

        public static bool DeleteBoardComponent(this Board board, int x, int y)
        {
            var comp = board.GetComponentAt(x, y);

            if (comp != null)
            {
                StuffDeleter.DeleteThing(comp);
                return true;
            }

            return false;
        }

        public static void SetBoardComponent(this Board board, int x, int y, CircuitComponent component, float rotation = 0)
        {
            board.DeleteBoardComponent(x, y);
            board.AddBoardComponent(component, x, y, rotation);
        }

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
