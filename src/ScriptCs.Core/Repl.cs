using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl : ScriptExecutor
    {
        private readonly string[] _scriptArgs;

        private readonly IObjectSerializer _serializer;

        private readonly IInputHistory _inputHistory;

        public Repl(
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptEngine scriptEngine,
            IObjectSerializer serializer,
            ILog logger,
            IConsole console,
            IFilePreProcessor filePreProcessor,
            IInputHistory inputHistory
        ) : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
            _scriptArgs = scriptArgs;
            _serializer = serializer;
            _inputHistory = inputHistory;
            Console = console;
        }

        public string Buffer { get; set; }

        public IConsole Console { get; private set; }

        public override void Terminate()
        {
            base.Terminate();
            Logger.Debug("Exiting console");
            Console.Exit();
        }

        public override ScriptResult Execute(string script, params string[] scriptArgs)
        {
            Guard.AgainstNullArgument("script", script);

            try
            {
                if (script.StartsWith(":dump", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = script.Substring(":dump".Length).Trim();
                    var filePath = String.IsNullOrWhiteSpace(arg) ? "dump.csx" : arg;

                    var inputLines = _inputHistory.BuildHistory();
                    _inputHistory.Clear();

                    FileSystem.WriteToFile(filePath, inputLines);

                    return new ScriptResult();
                }

                if (script.StartsWith("#clear", StringComparison.OrdinalIgnoreCase) ||
                    script.StartsWith(":clear", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    _inputHistory.Clear();
                    return new ScriptResult();
                }

                if (script.StartsWith("#reset"))
                {
                    Reset();
                    return new ScriptResult();
                }

                var preProcessResult = FilePreProcessor.ProcessScript(script);

                ImportNamespaces(preProcessResult.Namespaces.ToArray());

                foreach (var reference in preProcessResult.References)
                {
                    var referencePath = FileSystem.GetFullPath(Path.Combine(Constants.BinFolder, reference));
                    AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
                }

                Console.ForegroundColor = ConsoleColor.Cyan;

                Buffer += preProcessResult.Code;

                var result = ScriptEngine.Execute(Buffer, _scriptArgs, References, DefaultNamespaces, ScriptPackSession);
                if (result == null)
                {
                    _inputHistory.AddLine(script);
                    _inputHistory.Commit();
                    return new ScriptResult();
                }

                if (result.CompileExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.CompileExceptionInfo.SourceException.Message);
                }

                if (result.ExecuteExceptionInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.ExecuteExceptionInfo.SourceException.Message);
                }

                if (result.IsPendingClosingChar)
                {
                    _inputHistory.AddLine(script);

                    return result;
                }

                if (!result.IsPendingClosingChar)
                {
                    if (result.CompileExceptionInfo != null) _inputHistory.Rollback();    
                    else 
                    {
                        _inputHistory.AddLine(script);
                    }
                }

                if (result.ReturnValue != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    var serializedResult = _serializer.Serialize(result.ReturnValue);

                    Console.WriteLine(serializedResult);
                }

                _inputHistory.Commit();
                Buffer = null;
                return result;
            }
            catch (FileNotFoundException fileEx)
            {
                RemoveReferences(fileEx.FileName);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + fileEx + "\r\n");
                return new ScriptResult { CompileExceptionInfo = ExceptionDispatchInfo.Capture(fileEx) };
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + ex + "\r\n");
                return new ScriptResult { ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex) };
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
