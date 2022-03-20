using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace JHW.VersionControl
{
    internal class LinkedCharsDoc : LinkedListDoc<char>
    {
        public LinkedCharsDoc() { }
        public LinkedCharsDoc(string filename) 
        {
            string text = File.ReadAllText(filename);
            foreach (char c in text)
            {
                _linkedList.AddLast(c);
            }
        }
        
        public override void Save(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            StreamWriter sw = new StreamWriter(filename);
            foreach (char c in _linkedList)
            {
                sw.Write(c);
            }
            sw.Close();
        }
    }
}
