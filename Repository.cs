using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace JHW.VersionControl
{
    public static class Repository<T>
    {
        public const string RootBranchName = "b.1";
        private const string _repoSubDirectory = ".shrub";
        private const string _branchSubDirectory = "branch";
        private const string _binarySourceSubDirectory = "bin";

        private static readonly Dictionary<string, Branch<T>> _branchDict = 
            new Dictionary<string, Branch<T>>();

        private static string _workingDirectory;
        private static int _binaryFileNum = 1;

        #region private methods
        private static string RepoPath
        {
            get => Path.Combine(_workingDirectory, _repoSubDirectory);
        }

        private static string RepoBinPath
        {
            get => Path.Combine(RepoPath, _binarySourceSubDirectory);
        }

        private static string RepoBranchPath
        {
            get => Path.Combine(RepoPath, _branchSubDirectory);
        }
        
        private static Branch<T> Deserialize(string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            using Stream stream = new FileStream(filename, FileMode.Open);
            return (Branch<T>)formatter.Deserialize(stream);
        }

        private static Stack<ChangeSet<T>> StackOfChangeSets(string branchName, string filename)
        {
            Stack<ChangeSet<T>> result = new Stack<ChangeSet<T>>();

            while (branchName != null && _branchDict.ContainsKey(branchName))
            {
                result.Push(_branchDict[branchName].TextChangeSet(filename));
                branchName = ParentName(branchName);
            }

            return result;
        }
        #endregion

        public static void Serialize(string branchName, Branch<T> branch)
        {
            IFormatter formatter = new BinaryFormatter();
            string filename = Path.Combine(RepoBranchPath, branchName + ".bin");
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, branch);
            stream.Close();
        }

        public static Branch<T> GetBranch(string name)
        {
            return _branchDict[name];
        }

        public static bool ExistsRepoSubDirectory(string workingDirectory)
        {
            return Directory.Exists(Path.Combine(workingDirectory, _repoSubDirectory));
        }

        public static void PlantShrub(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            
            Directory.CreateDirectory(RepoBranchPath);
            Directory.CreateDirectory(RepoBinPath);

            Dictionary<string, string> binarySources = new Dictionary<string, string>();
            Dictionary<string, ChangeSet<T>> textChangeSets = new Dictionary<string, ChangeSet<T>>();

            foreach (var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                string relativeFilename = Path.GetRelativePath(_workingDirectory, filename);

                if (Utility.IsBinary(filename))
                {
                    string sourceFilename = Path.Combine(RepoBinPath, _binaryFileNum + ".bin");
                    File.Copy(filename, sourceFilename);
                     
                    binarySources.Add(relativeFilename, sourceFilename);
                    _binaryFileNum++;
                }
                else
                {
                    IDocument<T> tdoc = (IDocument<T>)new LinkedCharsDoc(filename);
                    
                    textChangeSets.Add(relativeFilename, tdoc.ToChangeSet());
                }
            }

            Branch<T> root = new Branch<T>(binarySources, textChangeSets, "trunk");

            _branchDict.Add(RootBranchName, root);
            Serialize(RootBranchName, root);
        }

        public static void LoadShrub(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            _binaryFileNum = 1 + Directory.GetFiles(RepoBinPath).Length;
            foreach (string filename in Directory.GetFiles(RepoBranchPath, "*.bin"))
            {
                var filenameOnly = Path.GetFileName(filename);
                var branchName = filenameOnly.Substring(0, filenameOnly.Length - 4);
                _branchDict.Add(branchName, Deserialize(filename));
            }
        }

        public static string NextAvailableChildName(string name)
        {
            int childNum = 1;
            string childName = Utility.ChildName(name, childNum);
            while (_branchDict.ContainsKey(childName))
            {
                childName = Utility.ChildName(name, ++childNum);
            }
            return childName;
        }

        public static string ParentName(string name)
        {
            if (name == RootBranchName)
            {
                return null;
            }

            var tokens = Utility.Split(name);

            if (tokens.LastToken == '.' &&
                tokens.LastNumber == 1)
            {
                return tokens.Prefix;
            }

            if (tokens.LastToken == '-')
            {
                tokens = Utility.Split(tokens.Prefix);
            }

            return tokens.Prefix + tokens.LastToken + (tokens.LastNumber - 1);
        }

        public static void ClearWorkingDirectory()
        {
            foreach (string path in Directory.GetDirectories(_workingDirectory))
            {
                if (Path.GetRelativePath(_workingDirectory, path) != ".shrub")
                {
                    Directory.Delete(path, true);
                }
            }
            foreach (string filename in Directory.GetFiles(_workingDirectory))
            {
                File.Delete(filename);
            }
        }

        // The restoration of the working directory
        // of a file on a branch.
        public static void Checkout(string branchName, string filename)
        {
            Branch<T> branch = _branchDict[branchName];

            if (branch.HasTextChangeSet(filename))
            {
                IDocument<T> doc = (IDocument<T>)new LinkedCharsDoc();
                var stack = StackOfChangeSets(branchName, filename);
                while (stack.Count > 0)
                {
                    doc.Apply(stack.Pop());
                }

                doc.Save(Path.Combine(_workingDirectory, filename));
            }
            else if (branch.HasBinarySource(filename))
            {
                File.Copy(branch.BinarySource(filename),
                    Path.Combine(_workingDirectory, filename));
            }
        }

        public static Stack<string> PreOrderTraversal(string branchName)
        {
            Stack<string> result = new Stack<string>();
            result.Push(branchName);

            int i = 1;
            string childName = Utility.ChildName(branchName, i);
            while (_branchDict.ContainsKey(childName))
            {
                var stack = PreOrderTraversal(childName);
                while (stack.Count > 0)
                {
                    result.Push(stack.Pop());
                }
                childName = Utility.ChildName(branchName, ++i);
            }
            
            return result;
        }




        //todo public static void Grow() {}
        public static void Grow(string parentName, string description)
        {
            Dictionary<string, string> binarySources = new Dictionary<string, string>();
            Dictionary<string, ChangeSet<T>> textChangeSets = new Dictionary<string, ChangeSet<T>>();

            //foreach file in the working directory
            //if file is binary or not
            foreach(var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                if (Utility.IsBinary(filename))
                {
                    
                }
                else
                {
                    //check if the parent has a changeset
                    if (_branchDict[parentName].HasTextChangeSet(filename))
                    {

                    }
                    else
                    {
                        IDocument<T> tdoc = (IDocument<T>) new LinkedCharsDoc(filename);
                        textChangeSets.Add(filename, tdoc.ToChangeSet());
                    }
                }
            }

            Branch<T> branch = new Branch<T>(binarySources, textChangeSets, description);
        }

        

        
    }
}
