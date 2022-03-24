using System;
using System.Collections.Generic;
using System.Text;

namespace JHW.VersionControl
{
    // Changesets are being persisted through binary serialization.
    // We have to be careful not to allow users of this object
    // to modify it and cause problems due to its persistent nature.
    // Thus, members are private and the one public method available does not
    // modify the object.

    [Serializable]
    public class ChangeSet<T>
    {
        private readonly List<(int Line, T Content)> _targetInsert;
        private readonly List<(int Line, int RecoverLine, int Continuation)> _sourceRecovery;

        public ChangeSet(List<(int, T)> targetInsert, List<(int, int, int)> sourceRecovery)
        {
            _targetInsert = targetInsert;
            _sourceRecovery = sourceRecovery;
        }

        public Document<T> Transition(Document<T> source)
        {
            Dictionary<int, T> dic = new Dictionary<int, T>();

            foreach (var (Line, Content) in _targetInsert)
            {
                dic.Add(Line, Content);
            }
            foreach (var (Line, RecoverLine, Continuation) in _sourceRecovery)
            {
                for (int i = 0; i < Continuation; i++)
                {
                    dic.Add(Line + i, source[RecoverLine + i]);
                }
            }

            return new Document<T>(dic);
        }
    }
}
