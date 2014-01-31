using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class InputHistory : IInputHistory
    {
        private StringBuilder _lines;

        private StringBuilder _currentLine;

        public InputHistory()
        {
            _lines = new StringBuilder();
            _currentLine = new StringBuilder();
        }

        public void AddLine(string line)
        {
            _currentLine.AppendLine(line);
        }

        public void Commit()
        {
            _lines.Append(_currentLine.ToString());
            _currentLine.Clear();
        }

        public void Rollback()
        {
            _currentLine.Clear();
        }

        public string BuildHistory()
        {
            return _lines.ToString();
        }

        public void Clear()
        {
            _lines.Clear();
            _currentLine.Clear();
        }
    }
}
