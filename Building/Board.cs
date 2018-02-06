using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    public class Board
    {
        private static int IdCounter = 100;

        public int Width { get; }
        public int Height { get; }
        public GameObject Object { get; }
        public int Id { get; }

        public Board() { }
        public Board(int w, int h, GameObject obj, int? id = null)
        {
            this.Width = w;
            this.Height = h;
            this.Object = obj;
            
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
    }
}
