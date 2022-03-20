using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JHW.VersionControl
{
    public static class Repository<T>
    {
        public const string RootBranchName = "b.1";
        private const string _shrubSubDir = ".shrub";
        private const string _branchSubDir = "branch";
        private const string _binarySourceSubDir = "bin";

        private static readonly Dictionary<string, Branch<T>> _branchDict = 
            new Dictionary<string, Branch<T>>();

        private static string _workingDirectory = null;
        private static int _binaryFileNum = 1;

        private static string ShrubPath
        {
            get => Path.Combine(_workingDirectory, _shrubSubDir);
        }

        private static string ShrubBinPath
        {
            get => Path.Combine(ShrubPath, _binarySourceSubDir);
        }

        public static string ShrubBranchPath
        {
            get => Path.Combine(ShrubPath, _branchSubDir);
        }

        private static Stack<ChangeSet<T>> StackOfChangeSets(string branchName, string filename)
        {
            Stack<ChangeSet<T>> result = new Stack<ChangeSet<T>>();

            while (_branchDict.ContainsKey(branchName))
            {
                result.Push(_branchDict[branchName].TextChangeSet(filename));
                branchName = Utility.ParentName(branchName);
            } 
            
            return result;
        }
        
        

        public static Branch<T> GetBranch(string name)
        {
            return _branchDict[name];
        }

        public static bool ExistsShrub(string path)
        {
            return Directory.Exists(Path.Combine(path, _shrubSubDir));
        }

        public static void PlantShrub(string path)
        {
            _workingDirectory = path;
            
            Directory.CreateDirectory(ShrubBranchPath);
            Directory.CreateDirectory(ShrubBinPath);

            Dictionary<string, string> binarySources = new Dictionary<string, string>();
            Dictionary<string, ChangeSet<T>> textChangeSets = new Dictionary<string, ChangeSet<T>>();

            foreach (var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                string relativeFilename = Path.GetRelativePath(_workingDirectory, filename);

                if (Utility.IsBinary(filename))
                {
                    string sourceFilename = Path.Combine(ShrubBinPath, _binaryFileNum + ".bin");
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
            root.Serialize(Path.Combine(ShrubBranchPath, RootBranchName + ".bin"));
            _branchDict.Add(RootBranchName, root);
        }

        public static void LoadShrub(string path)
        {
            _workingDirectory = path;
            _binaryFileNum = 1 + Directory.GetFiles(ShrubBinPath).Length;
            foreach (string filename in Directory.GetFiles(ShrubBranchPath, "*.bin"))
            {
                var filenameOnly = Path.GetFileName(filename);
                var branchName = filenameOnly.Substring(0, filenameOnly.Length - 4);
                _branchDict.Add(branchName, Branch<T>.Deserialize(filename));
            }
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

        public static string NextAvailableChildName(string parentName)
        {
            int childNum = 1;
            string childName = Utility.ChildName(parentName, childNum);
            while (_branchDict.ContainsKey(childName))
            {
                childName = Utility.ChildName(parentName, ++childNum);
            }
            return childName;
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
