using System.IO;
using System.Collections.Generic;

namespace JHW.VersionControl
{
    public class DocumentOfChars : Document<char>
    {
        public DocumentOfChars() { }
        public DocumentOfChars(string filename)
        {
            string text = File.ReadAllText(filename);
            for (int i = 0; i < text.Length; i++)
            {
                _dictionary.Add(i, text[i]);
            }
        }
    }
}
