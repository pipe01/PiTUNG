using PiTung_Bootstrap.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    public class Builder
    {
        public static Builder Instance { get; } = new Builder();

        private Builder() { }

        public void AddComponentToBoard(CircuitComponent component, int x, int y, int boardId, float rotation = 0)
        {
            AddComponentToBoard(component, x, y, BoardManager.Instance.GetBoard(boardId), rotation);
        }
        public void AddComponentToBoard(CircuitComponent component, int x, int y, Board board, float rotation = 0)
        {
            CircuitBoard circuit = board.Object.GetComponent<CircuitBoard>();

            GameObject gameObject = UnityEngine.Object.Instantiate(component.Prefab, new Vector3(10000f, 10000f, 10000f), Quaternion.identity, circuit.transform);
            gameObject.transform.localPosition = new Vector3(x + 0.5f, .25f, y + 0.5f) * 0.3f;
            
            var num = (float)(Mathf.RoundToInt(gameObject.transform.localEulerAngles.y / 90f) * 90);
            num += rotation;

            gameObject.transform.localEulerAngles = new Vector3(0, num, 0f);
            gameObject.AddComponent<SaveThisObject>().ObjectType = component.Name;

            StuffPlacer.DestroyIntersectingConnections(gameObject);
            MegaMesh.AddMeshesFrom(gameObject);
        }

        public void ConnectInputOutput(Board board, int inputX, int inputY, int outputX, int outputY)
        {
            var aObj = board.GetComponentAt(inputX, inputY);
            var bObj = board.GetComponentAt(outputX, outputY);

            if (aObj == null || bObj == null)
            {
                return;
            }

            var input = aObj.GetComponentInChildren<CircuitInput>();
            var output = bObj.GetComponentInChildren<Output>();

            if (output == null || input == null)
            {
                return;
            }

            StuffConnecter.CreateIOConnection(input, output);
        }
    }
}
