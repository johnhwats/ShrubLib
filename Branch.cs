using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JHW.VersionControl
{
    [Serializable]
    public class Branch
    {
        public readonly Dictionary<string, string> BinaryFileSource = 
            new Dictionary<string, string>();

        public readonly Dictionary<string, ChangeSet> TextFileChangeSet = 
            new Dictionary<string, ChangeSet>();

        public string Description;

        public Branch(string description)
        {
            Description = description;
        }
    }
}
