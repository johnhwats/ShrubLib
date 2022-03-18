using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    [Serializable]
    public class ChangeSet<T>
    {
        public readonly List<int> Keep;
        public readonly List<(int, T)> Insert;

        public ChangeSet(List<int> keep, List<(int, T)> insert)
        {
            Keep = keep;
            Insert = insert;
        }
    }
}
