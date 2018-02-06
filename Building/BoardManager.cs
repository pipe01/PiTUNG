using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

namespace PiTung_Bootstrap.Building
{
    public class BoardManager
    {
        public static BoardManager Instance { get; } = new BoardManager();

        private List<Board> Boards = new List<Board>();

        public delegate void BoardPlacedDelegate(Board board);
        public event BoardPlacedDelegate BoardPlaced;

        internal BoardManager() { }

        internal void BoardAdded(int x, int z, GameObject obj)
        {
            var b = new Board(x, z, obj);
            Boards.Add(b);
            BoardPlaced?.Invoke(b);
        }


    }
}
