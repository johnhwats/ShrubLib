using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    [Serializable]
    public class ChangeSet
    {
        public readonly List<int> KeepLines;
        public readonly List<(int, string)> InsertLines;

        public ChangeSet(List<int> linesToKeep, List<(int, string)> linesToInsert)
        {
            KeepLines = linesToKeep;
            InsertLines = linesToInsert;
        }

        /*
        public static ChangeSet Parse(string line)
        {
            string[] tokens = line.Split(';');
            string[] keepTokens = tokens[0].Split(',');
            int[] vs = new int[keepTokens.Length];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = int.Parse(keepTokens[i]);
            }
            string[] insertTokens = tokens[1].Split(',');
            (int, string)[] ps = new (int, string)[insertTokens.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = insertTokens[i].Trim()
            }
        }

        private static string[] SplitInsertLine(string line)
        {
            int parensLevel = 0;
            for (int i = 0; i < line.Length; i++)
            {
                switch(lines[i])
                { }
                if (line[i] == '(')
                {
                    parensLevel++;
                }
            }
        }

        public override string ToString()
        {
            // 1,2,3,4;(1,"ekwekjrlw"),(3,"dflksj")
            return null;
        }*/
    }
}
