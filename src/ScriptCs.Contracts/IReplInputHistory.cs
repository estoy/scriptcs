using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptCs.Contracts
{
    public interface IReplInputHistory
    {
        void AddLine(string line);
        void Commit();
        void Rollback();

        string BuildHistory();
        void Clear();
    }
}
