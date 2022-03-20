using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

        /*public static string GetRelativeFilename(string path, string filename)
        {
            string fileDir = Path.GetDirectoryName(filename);
            string relPath = Path.GetRelativePath(path, fileDir);
            return Path.Combine(relPath, Path.GetFileName(filename));
        }*/

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

        //"AGBAT" and "GAB"
        public static int[,] LCSTableString(string a, string b)
        {
            return LCSTable<string, char>(a, b, a.Length, b.Length);
        }

        public static int[,] LCSTable<S, T>(S a, S b, int x, int y) 
            where S : IEnumerable<T>
            where T : IEquatable<T>
        {
            int[,] c = new int[x + 1, y + 1];

            c[0, 0] = 0;
            int i = 0;
            foreach (T t in a)
            {
                c[i++, 0] = 0;
            }
            int j = 0;
            foreach (T t in b)
            {
                c[0, j++] = 0;
            }

            i = 0;
            j = 0;
            foreach (T t1 in a)
            {
                i++;
                foreach (T t2 in b)
                {
                    j++;
                    Console.WriteLine("[{0},{1}]", i, j);
                    if (t1.Equals(t2))
                    {
                        c[i, j] = c[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        c[i, j] = Math.Max(c[i, j - 1],
                            c[i - 1, j]);
                    }
                }
                j = 0;
            }
            return c;
        }
    }
}
