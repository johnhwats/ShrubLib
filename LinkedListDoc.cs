using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace JHW.VersionControl
{
    internal abstract class LinkedListDoc<T> : IDocument<T>
    {
        protected readonly LinkedList<T> _linkedList = new LinkedList<T>();

        public int Count => _linkedList.Count;

        public T this[int index]
        {
            get
            {
                LinkedListNode<T> node = _linkedList.First;
                while (node != null)
                {
                    if (index > 0)
                    {
                        index--;
                        node = node.Next;
                    }
                    else if (index == 0)
                    {
                        return node.Value;
                    }
                    else 
                    {
                        break;
                    }
                }
                throw new IndexOutOfRangeException();
            }
        }

        public LinkedListDoc()
        {
        }
        

        public abstract void Save(string filename);
        
        public ChangeSet<T> ToChangeSet()
        {
            List<(int, T)> inserts = new List<(int, T)>(_linkedList.Count);
            int i = 0;
            foreach (T item in _linkedList)
            {
                inserts.Add((i, item));
                i++;
            }
            return new ChangeSet<T>(new List<int>(), inserts);
        }

        public void DeleteFilter(List<int> keep)
        {
            int excludeIndex = 0;

            int nodeIndex = 0;
            LinkedListNode<T> node = _linkedList.First;

            while (node != null)
            {
                LinkedListNode<T> nextNode = node.Next;

                if (excludeIndex == keep.Count ||
                    (nodeIndex < keep[excludeIndex]))
                {
                    _linkedList.Remove(node);
                }
                else if (nodeIndex == keep[excludeIndex])
                {
                    excludeIndex++;
                }

                nodeIndex++;
                node = nextNode;
            }
        }

        public void Insert(List<(int Index, T Item)> insertions)
        {
            int insertIndex = 0;

            int nodeIndex = 0;
            LinkedListNode<T> node = _linkedList.First;

            while (node != null && insertIndex < insertions.Count)
            {
                if (nodeIndex == insertions[insertIndex].Index)
                {
                    _linkedList.AddBefore(node, insertions[insertIndex].Item);
                    insertIndex++;
                }
                else
                {
                    node = node.Next;
                }
                nodeIndex++;
            }

            while (insertIndex < insertions.Count && nodeIndex == insertions[insertIndex].Index)
            {
                _linkedList.AddLast(insertions[insertIndex].Item);
                insertIndex++;
                nodeIndex++;
            }
        }

        public void Apply(ChangeSet<T> cs)
        {
            DeleteFilter(cs.Keep);
            Insert(cs.Insert);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }
    }
}
