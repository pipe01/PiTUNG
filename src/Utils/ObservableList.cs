using System.Collections;
using System.Collections.Generic;

namespace PiTung
{
    internal class ObservableList<T> : IList<T>
    {
        private List<T> InternalList = new List<T>();

        public int Count => InternalList.Count;

        public bool IsReadOnly => false;

        public T this[int index] { get => InternalList[index]; set => InternalList[index] = value; }

        public delegate void ItemAddedDelegate(T item);
        public event ItemAddedDelegate ItemAdded;

        public void Add(T item)
        {
            InternalList.Add(item);
            ItemAdded?.Invoke(item);
        }

        public void Clear() => InternalList.Clear();

        public bool Contains(T item) => InternalList.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => InternalList.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => InternalList.GetEnumerator();

        public bool Remove(T item) => InternalList.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => InternalList.GetEnumerator();

        public int IndexOf(T item) => InternalList.IndexOf(item);

        public void Insert(int index, T item) => InternalList.Insert(index, item);

        public void RemoveAt(int index) => InternalList.RemoveAt(index);
    }
}
