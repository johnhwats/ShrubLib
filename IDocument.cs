using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    public interface IDocument<T> : IReadOnlyList<T>
    {
        void Save(bool byLine, string filename);
        ChangeSet<T> GetChangeSet(IDocument<T> source);
    }
}
