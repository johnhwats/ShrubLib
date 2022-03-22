using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    public interface IDocument<T> : IReadOnlyList<T>
    {
        void Apply(ChangeSet<T> changeSet);
        void Save(string filename);
        ChangeSet<T> ToChangeSet();
    }
}
