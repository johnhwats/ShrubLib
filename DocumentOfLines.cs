using System.IO;

namespace JHW.VersionControl
{
    public class DocumentOfLines : Document<string>
    {
        public DocumentOfLines() { }
        public DocumentOfLines(string filename)
        {
            string[] text = File.ReadAllLines(filename);
            for (int i = 0; i < text.Length; i++)
            {
                _dictionary.Add(i, text[i]);
            }
        }
    }
}
