using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptCs.Contracts
{
    public interface IInputHistory
    {
        void AddLine(string line);
        void Commit();
        //void CommitLine(string line);
        void Rollback();

        string BuildHistory();
        void Clear();

        //string BuildHistoryAndClear();
    }
}
