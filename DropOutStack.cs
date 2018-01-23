using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polyglot
{
    class DropOutStack<T>
    {
        private T[] items;
        private int top = 0;
        public int Count { get; private set; }

        public DropOutStack(int capacity)
        {
            Count = 0;
            items = new T[capacity];
        }

        public void Push(T item)
        {
            if (Count < items.Length - 1)
                Count++;
            items[top] = item;
            top = (top + 1) % items.Length;
        }

        public T Pop()
        {
            if (Count > 0)
            {
                top = (items.Length + top - 1) % items.Length;
                Count--;
                return items[top];
            }
            throw new InvalidOperationException("Attempting to pop empty stack");
        }

        public T Get(int index)
        {
            if (index < Count)
            {
                int _index = (items.Length + top - 1 - index) % items.Length;
                return items[_index];
            }
            throw new IndexOutOfRangeException($"Index out of range {index}");
        }
    }
}
