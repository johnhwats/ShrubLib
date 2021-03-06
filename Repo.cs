using System.Collections.Generic;
using System.IO;

namespace JHW.VersionControl
{
    public static class Repo<T>
    {
        public const string RootBranchName = "b.1";        
        private const string _branchSubDir = "branch";
        private const string _binarySourceSubDir = "bin";

        private static readonly Dictionary<string, Branch<T>> _branchDict = 
            new Dictionary<string, Branch<T>>();
        
        private static string _repoPath;
        private static int _binaryFileNum = 1;

        private static string ShrubBinPath
        {
            get => Path.Combine(_repoPath, _binarySourceSubDir);
        }

        public static string ShrubBranchPath
        {
            get => Path.Combine(_repoPath, _branchSubDir);
        }


        private static Stack<ChangeSet<T>> StackOfChangeSets(string branchName, string filename)
        {
            Stack<ChangeSet<T>> result = new Stack<ChangeSet<T>>();

            while (_branchDict.ContainsKey(branchName) &&
                _branchDict[branchName].HasTextChangeSet(filename))
            {
                result.Push(_branchDict[branchName].TextChangeSet(filename));
                branchName = Utility.ParentName(branchName);
            } 
            
            return result;
        }

        private static string NextAvailableChildName(string parentName)
        {
            int childNum = 1;
            string childName = Utility.ChildName(parentName, childNum);
            while (_branchDict.ContainsKey(childName))
            {
                childName = Utility.ChildName(parentName, ++childNum);
            }
            return childName;
        }

        private static IDocument<T> RestoreDocument(string branchName, string relativeFilename)
        {
            Document<T> doc = new Document<T>();
            
            var stack = StackOfChangeSets(branchName, relativeFilename);
            while (stack.Count > 0)
            {
                doc = stack.Pop().Transition(doc);
            }

            return doc;
        }

        
        public static Branch<T> GetBranch(string name)
        {
            return _branchDict[name];
        }

        public static void LoadShrub(string repoPath)
        {
            _repoPath = repoPath;
            _binaryFileNum = 1 + Directory.GetFiles(ShrubBinPath).Length;
            foreach (string filename in Directory.GetFiles(ShrubBranchPath, "*.bin"))
            {
                var filenameOnly = Path.GetFileName(filename);
                var branchName = filenameOnly.Substring(0, filenameOnly.Length - 4);
                _branchDict.Add(branchName, Branch<T>.Deserialize(filename));
            }
        }

        public static void Checkout(string workingDirectory, 
            bool byLine, string branchName, HashSet<string> filenames)
        {
            var branch = GetBranch(branchName);
            foreach (var fn in branch.TextRelativeFilenames)
            {
                if (filenames.Contains(fn))
                {
                    RestoreDocument(branchName, fn).Save(byLine, Path.Combine(workingDirectory, fn));
                }
            }
            foreach (var fn in branch.BinaryRelativeFilenames)
            {
                if (filenames.Contains(fn))
                {
                    File.Copy(branch.BinarySource(fn),
                        Path.Combine(workingDirectory, fn));
                }
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

        public static void PlantShrub(string repoPath, string workingDirectory, bool byLine)
        {
            _repoPath = repoPath;
            Directory.CreateDirectory(ShrubBranchPath);
            Directory.CreateDirectory(ShrubBinPath);
            SproutBranch(workingDirectory, byLine, null, "trunk");
        }


        public static int AnalyzeBranch(string parentName)
        {
            return -1;
        }

        public static void SproutBranch(string workingDirectory, 
            bool byLine, string parentName, string description = null)
        {
            Dictionary<string, string> binarySources = 
                new Dictionary<string, string>();
            Dictionary<string, ChangeSet<T>> textChangeSets = 
                new Dictionary<string, ChangeSet<T>>();

            foreach (var filename in Utility.GetFilesRecursively(workingDirectory))
            {
                string relativeFilename = Path.GetRelativePath(workingDirectory, filename);

                if (Utility.IsBinary(filename))
                {
                    if (parentName != null &&
                        _branchDict.ContainsKey(parentName) &&
                        _branchDict[parentName].HasBinarySource(relativeFilename))
                    {
                        string parentSourceFilename =
                            _branchDict[parentName].BinarySource(relativeFilename);
                        FileInfo parentSourceInfo = new FileInfo(parentSourceFilename);

                        FileInfo fileInfo = new FileInfo(filename);

                        if (fileInfo.LastWriteTimeUtc == parentSourceInfo.LastWriteTimeUtc &&
                            fileInfo.Length == parentSourceInfo.Length)
                        {
                            binarySources.Add(relativeFilename, parentSourceFilename);
                        }
                    }

                    if (!binarySources.ContainsKey(relativeFilename))
                    {
                        string sourceFilename = Path.Combine(ShrubBinPath, _binaryFileNum + ".bin");
                        _binaryFileNum++;
                        File.Copy(filename, sourceFilename);
                        binarySources.Add(relativeFilename, sourceFilename);
                    }
                }
                else
                {
                    if (parentName != null &&
                        _branchDict.ContainsKey(parentName) &&
                        _branchDict[parentName].HasTextChangeSet(relativeFilename))
                    {
                        IDocument<T> restoration = RestoreDocument(parentName, relativeFilename);

                        //todo create a change set through diffing
                        //and add to textChangeSet
                    }

                    if (!textChangeSets.ContainsKey(relativeFilename))
                    {
                        IDocument<T> tdoc;
                        if (byLine)
                        {
                            tdoc = (IDocument<T>) new DocumentOfLines(filename);
                        }
                        else
                        {
                            tdoc = (IDocument<T>)new DocumentOfChars(filename);
                        }

                        textChangeSets.Add(relativeFilename, tdoc.GetChangeSet(tdoc));
                    }
                }
            }
            var b = new Branch<T>(binarySources, textChangeSets, description);
            string bName = RootBranchName;
            if (parentName != null)
            {
                bName = NextAvailableChildName(parentName);
            }
            _branchDict.Add(bName, b);
            b.Serialize(Path.Combine(ShrubBranchPath, bName + ".bin"));
        }
    }
}
