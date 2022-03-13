using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace JHW.VersionControl
{
    public class TextDoc : IDocument
    {
        private LinkedList<string> _content = new LinkedList<string>();


        public TextDoc() { }
        public TextDoc(string filename) 
        {
            Load(filename);
        }
        

        private static LinkedList<string> Split(string text)
        {
            LinkedList<string> result = new LinkedList<string>();

            int lineStart = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' || text[i] == '\n')
                {
                    if (i > lineStart)
                    {
                        result.AddLast(text.Substring(lineStart, i - lineStart));
                    }
                    lineStart = i + 1;
                }
            }

            if (text.Length > lineStart)
                result.AddLast(text.Substring(lineStart, text.Length - lineStart));

            return result;
        }

        public ChangeSet ToChangeSet()
        {
            List<(int, string)> inserts = new List<(int, string)>(_content.Count);
            int i = 0;
            foreach (string line in _content)
            {
                inserts.Add((i, line));
                i++;
            }
            return new ChangeSet(new List<int>(), inserts);
        }

        public void DeleteFilter(List<int> keep)
        {
            int excludeIndex = 0;

            int nodeIndex = 0;
            LinkedListNode<string> node = _content.First;

            while (node != null)
            {
                LinkedListNode<string> nextNode = node.Next;

                if (excludeIndex == keep.Count ||
                    (nodeIndex < keep[excludeIndex]))
                {
                    _content.Remove(node);
                }
                else if (nodeIndex == keep[excludeIndex])
                {
                    excludeIndex++;
                }

                nodeIndex++;
                node = nextNode;
            }
        }

        public void Insert(List<(int Index, string Line)> insertions)
        {
            int insertIndex = 0;

            int nodeIndex = 0;
            LinkedListNode<string> node = _content.First;

            while (node != null && insertIndex < insertions.Count)
            {
                if (nodeIndex == insertions[insertIndex].Index)
                {
                    _content.AddBefore(node, insertions[insertIndex].Line);
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
                _content.AddLast(insertions[insertIndex].Line);
                insertIndex++;
                nodeIndex++;
            }
        }

        public void Apply(ChangeSet cs)
        {
            DeleteFilter(cs.KeepLines);
            Insert(cs.InsertLines);
        }


        public IEnumerator<string> GetEnumerator()
        {
            return _content.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _content.GetEnumerator();
        }

        private void Load(string filename)
        {
            string text = File.ReadAllText(filename);
            _content = Split(text);
        }

        public void Save(string filename)
        {
            File.WriteAllLines( filename, _content);
        }
    }
}
