using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

namespace PiTung_Bootstrap.Building
{
    public class BoardManager
    {
        public static BoardManager Instance { get; } = new BoardManager();

        private List<Board> Boards = new List<Board>();

        public delegate void BoardPlacedDelegate(int boardW, int boardH);
        public event BoardPlacedDelegate BoardPlaced;

        internal BoardManager() { }

        internal void BoardAdded(int x, int z, GameObject obj)
        {
            Boards.Add(new Board(x, z, obj));
            BoardPlaced?.Invoke(x, z);
        }


    }
}
