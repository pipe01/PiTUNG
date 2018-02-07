using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    public class Board
    {
        internal static int IdCounter = 1;

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

        public GameObject GetComponentAt(int x, int y)
        {
            foreach (var item in Object.GetComponentsInChildren<Transform>())
            {
                if (item.parent != Object.transform)
                    continue;

                var obj = item.gameObject;

                var ax = Mathf.RoundToInt((obj.transform.localPosition.x - 0.5f) / 0.3f) + 1;
                var ay = Mathf.RoundToInt((obj.transform.localPosition.z - 0.5f) / 0.3f) + 1;

                if (ax == x && ay == y)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}
