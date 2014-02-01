using Should;
using System;

namespace ScriptCs.Tests
{
    using Xunit;

    public class ReplInputHistoryTests
    {
        public class TheBuildHistoryMethod
        {
            private ReplInputHistory _history;

            public TheBuildHistoryMethod()
            {
                _history = new ReplInputHistory();
            }

            [Fact]
            public void ShouldReturnEmptyStringWhenNothingIsCommited()
            {
                _history.AddLine("line0");

                var result = _history.BuildHistory();

                result.ShouldBeEmpty();
            }

            [Fact]
            public void ShouldReturnAllCommittedLines()
            {
                _history.AddLine("line0");
                _history.AddLine("line1");
                _history.Commit();

                var result = _history.BuildHistory().Trim();

                result.ShouldEqual("line0" + Environment.NewLine + "line1");
            }

            [Fact]
            public void ShouldNotReturnLinesThatHaveBeenRolledBack()
            {
                _history.AddLine("line0");
                _history.Commit();
                _history.AddLine("to be rolled back");
                _history.Rollback();
                _history.AddLine("line1");
                _history.Commit();

                var result = _history.BuildHistory().Trim();

                result.ShouldEqual("line0" + Environment.NewLine + "line1");
            }

            [Fact]
            public void ShouldReturnEmptyStringAfterReplHistoryHasBeenCleared()
            {
                _history.AddLine("line0");
                _history.Commit();
                _history.Clear();

                var result = _history.BuildHistory();

                result.ShouldBeEmpty();
            }
        }
    }
}
