using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JHW.VersionControl
{
    public static class Utility
    {
        public static (string Prefix, char LastToken, int LastNumber) Split(
            string name)
        {
            for (int i = name.Length - 1; i > -1; i--)
            {
                if (name[i] == '.' || name[i] == '-')
                {
                    return (name.Substring(0, i),
                        name[i], int.Parse(name.Substring(i + 1)));
                }
            }
            throw new ArgumentException();
        }

        private static string FirstChildName(string name)
        {
            var tokens = Utility.Split(name);
            if (tokens.LastToken == '-')
            {
                return tokens.Prefix + tokens.LastToken + tokens.LastNumber + ".1";
            }
            else
            {
                return tokens.Prefix + tokens.LastToken + (tokens.LastNumber + 1);
            }
        }

        public static string ChildName(string name, int num)
        {
            if (num == 1)
            {
                return FirstChildName(name);
            }
            else
            {
                return FirstChildName(name) + '-' + (num - 1);
            }
        }

        public static string ParentName(string name)
        {
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

        public static List<string> GetFilesRecursively(string path)
        {
            List<string> result = new List<string>();
            result.AddRange(Directory.GetFiles(path));

            foreach (string dir in Directory.GetDirectories(path))
            {
                result.AddRange(GetFilesRecursively(dir));
            }
            return result;
        }

        public static void Clear(string dir, string excludeSubDir)
        {
            foreach (string subPath in Directory.GetDirectories(dir))
            {
                if (!Path.Equals(subPath, excludeSubDir))
                {
                    Directory.Delete(subPath, true);
                }
            }
            foreach (string filename in Directory.GetFiles(dir))
            {
                File.Delete(filename);
            }
        }

        // The source code was taken from "bytedev" on Stack Overflow.
        public static bool IsBinary(string filePath, int requiredConsecutiveNul = 1)
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';

            int nulCount = 0;

            using (var streamReader = new StreamReader(filePath))
            {
                for (var i = 0; i < charsToCheck; i++)
                {
                    if (streamReader.EndOfStream)
                        return false;

                    if ((char)streamReader.Read() == nulChar)
                    {
                        nulCount++;

                        if (nulCount >= requiredConsecutiveNul)
                            return true;
                    }
                    else
                    {
                        nulCount = 0;
                    }
                }
            }
            return false;
        }

        // Backtrack from Longest Common Subsequence via Wikipedia.
        public static List<int> RecursiveBacktrack<S,T>(int[,] c, S aStr, S bStr, int x, int y)
            where S : IReadOnlyList<T>
            where T : IEquatable<T>
        {
            if (x == 0 || y == 0)
            {
                return new List<int>();
            }
            else if (aStr[x - 1].Equals(bStr[y - 1]))
            {
                List<int> result = RecursiveBacktrack<S, T>(c, aStr, bStr, x - 1, y - 1);
                result.Add(x - 1);
                return result;
            }
            else if (c[x, y - 1] > c[x - 1, y])
            {
                return RecursiveBacktrack<S, T>(c, aStr, bStr, x, y - 1);
            }
            else
            {
                return RecursiveBacktrack<S, T>(c, aStr, bStr, x - 1, y);
            }
        }

        //todo maybe return list instead of stack
        //especially since the backtrack result
        //will be used multiple times
        public static Stack<int> BacktrackStack<S, T>(int[,] c, S aStr, S bStr, int x, int y)
            where S : IReadOnlyList<T>
            where T : IEquatable<T>
        {
            Stack<int> result = new Stack<int>();
            while (x > 0 && y > 0)
            {
                if (aStr[x - 1].Equals(bStr[y - 1]))
                {
                    --y;
                    result.Push(--x);
                }
                else if (c[x, y - 1] > c[x - 1, y])
                {
                    --y;
                }
                else
                {
                    --x;
                }
            }
            return result;
        }

        //todo the insert list will be obtained by traversing the
        //target document 

        //"AGBAT" and "GAB"
        public static int[,] LCSTable(char[] a, char[] b)
        {
            return LCSTable<char[], char>(a, b);
        }

        public static int[,] LCSTable<T>(IDocument<T> a, IDocument<T> b)
            where T : IEquatable<T>
        {
            return LCSTable<IDocument<T>, T>(a, b);
        }

        public static int[,] LCSTable<S, T>(S a, S b) 
            where S : IReadOnlyList<T>
            where T : IEquatable<T>
        {
            int m = a.Count;
            int n = b.Count;
            int[,] c = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                c[i, 0] = 0;
            }
            for (int j = 0; j <= n; j++)
            {
                c[0, j] = 0;
            }

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (a[i - 1].Equals(b[j - 1]))
                    {
                        c[i, j] = c[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        c[i, j] = Math.Max(c[i, j - 1],
                            c[i - 1, j]);
                    }
                }
            }
            return c;
        }
    }
}
