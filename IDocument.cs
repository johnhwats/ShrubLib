using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    public interface IDocument : IEnumerable<string>
    {
        /*int LineCount();
        string Line(int lineNumber);*/
        void Apply(ChangeSet changeSet);
        void Save(string filename);
        ChangeSet ToChangeSet();
    }
}
