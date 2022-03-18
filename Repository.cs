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
        public const string RootBranchName = "v.1";
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
        
        private static void Serialize(string branchName, Branch<T> branch)
        {
            IFormatter formatter = new BinaryFormatter();
            string filename = Path.Combine(RepoBranchPath, branchName + ".bin");
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, branch);
            stream.Close();
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

            while (_branchDict.ContainsKey(branchName))
            {
                result.Push(_branchDict[branchName].TextChangeSet(filename));
                branchName = ParentName(branchName);
            }

            return result;
        }
        #endregion

        public static Branch<T> GetBranch(string name)
        {
            return _branchDict[name];
        }

        public static bool ExistsRepoSubDirectory(string workingDirectory)
        {
            return Directory.Exists(Path.Combine(workingDirectory, _repoSubDirectory));
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

        public static void PlantShrub(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            //Directory.CreateDirectory(RepoPath);
            Directory.CreateDirectory(RepoBranchPath);
            Directory.CreateDirectory(RepoBinPath);

            Dictionary<string, string> binarySources = new Dictionary<string, string>();
            Dictionary<string, ChangeSet<T>> textChangeSets = new Dictionary<string, ChangeSet<T>>();

            foreach (var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                // get relative Filename
                if (Utility.IsBinary(filename))
                {
                    /*string sourceFilename = Path.Combine(WorkingDirectory,
                            ".shrub", Path.GetRelativePath(WorkingDirectory, filename));*/

                    string sourceFilename = Path.Combine(RepoBinPath, _binaryFileNum + ".bin");
                    File.Copy(filename, sourceFilename);
                     
                    binarySources.Add(filename, sourceFilename);
                    _binaryFileNum++;
                }
                else
                {
                    IDocument<T> tdoc = (IDocument<T>)new LinkedCharsDoc(filename);
                    
                    textChangeSets.Add(filename, tdoc.ToChangeSet());
                }
            }

            Branch<T> root = new Branch<T>(binarySources, textChangeSets, "trunk");

            _branchDict.Add(RootBranchName, root);
            Serialize(RootBranchName, root);

        }


        public static void Open(string workingDirectory)
        {
            _workingDirectory = workingDirectory;

            foreach (string filename in Directory.GetFiles(RepoPath, "*.br"))
            {
                string filenameOnly = Path.GetFileName(filename);
                _branchDict.Add(filenameOnly.Substring(0, filename.Length - 3),
                    Deserialize(filename));
            }

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

                
        // A restoration of the working directory
        // to a previous branch.
        public static void Inspect(string branchName)
        {
            Directory.Delete(_workingDirectory, true);
            
            Branch<T> branch = _branchDict[branchName];
            foreach (string filename in branch.TextFiles)
            {
                IDocument<T> doc = (IDocument<T>) new LinkedCharsDoc();
                var stack = StackOfChangeSets(branchName, filename);
                while (stack.Count > 0)
                {
                    doc.Apply(stack.Pop());
                }
                doc.Save(filename);
            }
            foreach (string filename in branch.BinaryFiles)
            {
                File.Copy(branch.BinarySource(filename), filename);
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
