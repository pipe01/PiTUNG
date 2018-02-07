using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    public static class Builder
    {
        public static void AddBoardComponent(this Board board, CircuitComponent component, int x, int y, float rotation = 0)
        {
            CircuitBoard circuit = board.Object.GetComponent<CircuitBoard>();

            GameObject gameObject = Object.Instantiate(component.Prefab, new Vector3(10000f, 10000f, 10000f), Quaternion.identity, circuit.transform);
            gameObject.transform.localPosition = new Vector3(x + 0.5f, .25f, y + 0.5f) * 0.3f;
            
            var num = (float)(Mathf.RoundToInt(gameObject.transform.localEulerAngles.y / 90f) * 90);
            num += rotation;

            gameObject.transform.localEulerAngles = new Vector3(0, num, 0f);
            gameObject.AddComponent<SaveThisObject>().ObjectType = component.Name;

            StuffPlacer.DestroyIntersectingConnections(gameObject);
            MegaMesh.AddMeshesFrom(gameObject);
        }

        public static void ConnectInputOutput(this Board board, int inputX, int inputY, int outputX, int outputY)
        {
            var input = GetComponentComponent<CircuitInput>(board, inputX, inputY);
            var output = GetComponentComponent<Output>(board, outputX, outputY);

            StuffConnecter.CreateIOConnection(input, output);
        }

        public static void ConnectInputInput(this Board board, int aX, int aY, int bX, int bY)
        {
            var a = GetComponentComponent<CircuitInput>(board, aX, aY);
            var b = GetComponentComponent<CircuitInput>(board, bX, bY);

            StuffConnecter.CreateIIConnection(a, b);
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
