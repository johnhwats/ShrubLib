using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JHW.VersionControl
{
    [Serializable]
    public class Branch<T>
    {
        private readonly Dictionary<string, string> _binarySources = 
            new Dictionary<string, string>();
        private readonly Dictionary<string, ChangeSet<T>> _textChangeSets = 
            new Dictionary<string, ChangeSet<T>>();

        public string Description { get; set; }

        public IEnumerable<string> BinaryFiles
        {
            get => _binarySources.Keys;
        }
        public IEnumerable<string> TextFiles
        {
            get => _textChangeSets.Keys;
        }


        public Branch(Dictionary<string, string> binarySources, 
            Dictionary<string, ChangeSet<T>> textChangeSets, string description = null)
        {
            _binarySources = binarySources;
            _textChangeSets = textChangeSets;
            Description = description;
        }

        public bool HasBinarySource(string filename)
        {
            return _binarySources.ContainsKey(filename);
        }

        public string BinarySource(string filename)
        {
            return _binarySources[filename];
        }

        public bool HasTextChangeSet(string filename)
        {
            return _textChangeSets.ContainsKey(filename);
        }

        public ChangeSet<T> TextChangeSet(string filename)
        {
            return _textChangeSets[filename];
        }

    }
}
