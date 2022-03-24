using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace JHW.VersionControl
{
    public class Document<T> : IDocument<T>
    {
        protected readonly Dictionary<int, T> _dictionary = new Dictionary<int, T>();

        public T this[int index] => _dictionary[index];
        public int Count => _dictionary.Count;

        public Document() { }
        public Document(Dictionary<int, T> dictionary)
        {
            _dictionary = dictionary;
        }

      
        public void Save(bool byLine, string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < Count; i++)
            {
                if (byLine)
                {
                    sw.WriteLine(this[i]);
                }
                else
                {
                    sw.Write(this[i]);
                }
            }
            sw.Close();
        }
        
        public ChangeSet<T> GetChangeSet(IDocument<T> source)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }
    }
}
