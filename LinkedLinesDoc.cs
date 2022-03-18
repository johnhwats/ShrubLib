using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace JHW.VersionControl
{
    internal class LinkedLinesDoc : LinkedListDoc<string>
    {
        public LinkedLinesDoc() { }
        public LinkedLinesDoc(string filename) 
        {
            // Instead of calling File.ReadAllLines, I thought
            // parsing the lines myself might be more efficient
            // because the intermediate step of creating
            // an array of strings is skipped.
            
            string text = File.ReadAllText(filename);
            
            int lineStart = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' || text[i] == '\n')
                {
                    if (i > lineStart)
                    {
                        _linkedList.AddLast(text[lineStart..i]);
                    }
                    lineStart = i + 1;
                }
            }

            if (text.Length > lineStart)
            {
                _linkedList.AddLast(text[lineStart..]);
            }
        }

        public override void Save(string filename)
        {
            File.WriteAllLines( filename, _linkedList);
        }
    }
}
