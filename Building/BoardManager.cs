using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace PiTung.Building
{
    /// <summary>
    /// Manages the world's boards.
    /// </summary>
    public class BoardManager
    {
        /// <summary>
        /// <see cref="BoardManager"/>'s singleton instance.
        /// </summary>
        public static BoardManager Instance { get; } = new BoardManager();


        private List<Board> Boards = new List<Board>();
        private Dictionary<int, int> InstanceIds = new Dictionary<int, int>();

        public delegate void BoardPlacedDelegate(Board board);
        /// <summary>
        /// Fires when a board is placed in the world. Doesn't fire when a board is moved.
        /// </summary>
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

        /// <summary>
        /// Gets a board with an ID.
        /// </summary>
        /// <param name="id">The board's ID</param>
        /// <returns>A board with ID <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentException">Throws when the board with ID <paramref name="id"/> doesn't exist.</exception>
        public Board GetBoard(int id)
        {
            if (TryGetBoard(id, out var b))
                return b;

            throw new ArgumentException($"Board with ID {id} not found.", nameof(id));
        }

        /// <summary>
        /// Tries to get a board with ID <paramref name="id"/>
        /// </summary>
        /// <param name="id">The board's ID.</param>
        /// <param name="board">The found board.</param>
        /// <returns>True if a board is found.</returns>
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

        /// <summary>
        /// Tries to get a <see cref="Board"/> object that represents an already loaded phyisical board.
        /// </summary>
        /// <param name="gameObject">The board's game object.</param>
        /// <param name="board">The resulted board.</param>
        /// <returns>True if the board is found.</returns>
        public bool TryGetExistingBoardFromGameObject(GameObject gameObject, out Board board)
        {
            if (InstanceIds.TryGetValue(gameObject.GetInstanceID(), out int id))
            {
                var boardComp = gameObject.GetComponent<CircuitBoard>();

                board = new Board(boardComp.x, boardComp.z, gameObject, id);
                return true;
            }

            board = null;
            return false;
        }
    }
}
