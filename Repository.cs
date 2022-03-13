using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;

namespace JHW.VersionControl
{
    public static class Repository
    {
        public const string RootBranchName = "v.1";
        
        public static readonly Dictionary<string, Branch> BranchMap = 
            new Dictionary<string, Branch>();

        //private static int _file
        private static string _workingDirectory;
        

        private static string RepoDirectory
        {
            get => Path.Combine(_workingDirectory, ".shrub");
        }

        private static void Serialize(string branchName, Branch branch)
        {
            IFormatter formatter = new BinaryFormatter();
            string filename = Path.Combine(RepoDirectory, branchName + ".bin");
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, branch);
            stream.Close();
        }

        private static Branch Deserialize(string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            using Stream stream = new FileStream(filename, FileMode.Open);
            return (Branch)formatter.Deserialize(stream);
        }

        public static string NextAvailableChildName(string name)
        {
            int childNum = 1;
            string childName = Utility.ChildName(name, childNum);
            while (BranchMap.ContainsKey(childName))
            {
                childName = Utility.ChildName(name, ++childNum);
            }
            return childName;
        }

        private static Stack<ChangeSet> ChangeStack(string branchName, string filename)
        {
            Stack<ChangeSet> result = new Stack<ChangeSet>();

            while (BranchMap.ContainsKey(branchName) &&
                BranchMap[branchName].TextFileChangeSet.TryGetValue(filename, out ChangeSet set))
            {
                result.Push(set);
                branchName = ParentName(branchName);
            }

            return result;
        }

        public static void Load(string workingDirectory)
        {
            _workingDirectory = workingDirectory;

            foreach (string filename in Directory.GetFiles(RepoDirectory, "*.br"))
            {
                string filenameOnly = Path.GetFileName(filename);
                BranchMap.Add(
                    filenameOnly.Substring(0, filename.Length - 3),
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
            
            Branch branch = BranchMap[branchName];
            foreach (string filename in branch.TextFileChangeSet.Keys)
            {
                IDocument doc = new TextDoc();
                var stack = ChangeStack(branchName, filename);
                while (stack.Count > 0)
                {
                    doc.Apply(stack.Pop());
                }
                doc.Save(filename);
            }
            foreach (string filename in branch.BinaryFileSource.Keys)
            {
                File.Copy(branch.BinaryFileSource[filename], filename);
            }
        }

        public static Stack<string> PreOrderTraversal(string branchName)
        {
            Stack<string> result = new Stack<string>();
            result.Push(branchName);

            int i = 1;
            string childName = Utility.ChildName(branchName, i);
            while (BranchMap.ContainsKey(childName))
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
            Branch branch = new Branch(description);

            //foreach file in the working directory
            //if file is binary or not
            foreach(var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                if (Utility.IsText(filename))
                {
                    //check if the parent has a changeset
                    if (BranchMap[parentName].TextFileChangeSet.ContainsKey(filename))
                    {

                    }
                    else
                    {
                        IDocument tdoc = new TextDoc(filename);
                        branch.TextFileChangeSet.Add(filename, tdoc.ToChangeSet());
                    }
                }
                else
                {

                }
            }
        }
        
        public static void Plant(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            
            Branch root = new Branch("");
            foreach (var filename in Utility.GetFilesRecursively(_workingDirectory))
            {
                if (Utility.IsText(filename))
                {
                    IDocument tdoc = new TextDoc(filename);
                    root.TextFileChangeSet.Add(filename, tdoc.ToChangeSet());
                }
                else
                {
                    //todo the source file name
                    //should maybe saved as "1.bin"
                    //"2.bin", "3.bin", and so on...
                    string sourceFilename = Path.Combine(_workingDirectory,
                            "shrub", Path.GetRelativePath(_workingDirectory, filename));
                    File.Copy(filename, sourceFilename);
                    root.BinaryFileSource.Add(filename, sourceFilename);
                }
            }

            BranchMap.Add(RootBranchName, root);
            Serialize(RootBranchName, root);
        }




    }
}
