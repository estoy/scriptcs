using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplInputHistory : IReplInputHistory
    {
        private readonly StringBuilder _lines;
        private readonly StringBuilder _currentLine;

        public ReplInputHistory()
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
            _lines.Append(_currentLine);
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
