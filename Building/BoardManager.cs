using PiTung_Bootstrap.Console;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

namespace PiTung_Bootstrap.Building
{
    public class BoardManager
    {
        public static BoardManager Instance { get; } = new BoardManager();

        private List<Board> Boards = new List<Board>();
        private Dictionary<int, int> InstanceIds = new Dictionary<int, int>();

        public delegate void BoardPlacedDelegate(Board board);
        public event BoardPlacedDelegate BoardPlaced;

        internal BoardManager() { }

        internal void BoardAdded(int x, int z, GameObject obj)
        {
            int? id = null;
            if (InstanceIds.TryGetValue(obj.GetInstanceID(), out var v))
            {
                id = v;
            }

            var b = new Board(x, z, obj, id);
            
            if (TryGetBoard(b.Id, out var _))
                return;

            Boards.Add(b);
            BoardPlaced?.Invoke(b);

            InstanceIds[obj.GetInstanceID()] = b.Id;
        }

        internal void Reset()
        {
            Board.IdCounter = 1;
            InstanceIds.Clear();
            Boards.Clear();
        }

        public Board GetBoard(int id)
        {
            if (TryGetBoard(id, out var b))
                return b;

            throw new ArgumentException($"Board with ID {id} not found.", nameof(id));
        }

        public bool TryGetBoard(int id, out Board board)
        {
            try
            {
                var b = Boards.SingleOrDefault(o => o.Id == id);
                board = b;
                return b != null;
            }
            catch (InvalidOperationException)
            {
                board = null;
                return false;
            }
        }
    }
}
