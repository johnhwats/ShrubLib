using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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


        public IEnumerable<string> BinaryRelativeFilenames
        {
            get => _binarySources.Keys;
        }
        public IEnumerable<string> TextRelativeFilenames
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

        public static Branch<T> Deserialize(string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            using Stream stream = new FileStream(filename, FileMode.Open);
            return (Branch<T>)formatter.Deserialize(stream);
        }

        public void Serialize(string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, this);
            stream.Close();
        }

        public bool HasBinarySource(string relativeFilename)
        {
            return _binarySources.ContainsKey(relativeFilename);
        }

        public string BinarySource(string relativeFilename)
        {
            return _binarySources[relativeFilename];
        }

        public bool HasTextChangeSet(string relativeFilename)
        {
            return _textChangeSets.ContainsKey(relativeFilename);
        }

        public ChangeSet<T> TextChangeSet(string relativeFilename)
        {
            return _textChangeSets[relativeFilename];
        }

    }
}
