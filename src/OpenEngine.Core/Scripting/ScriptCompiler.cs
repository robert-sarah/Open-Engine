// Created By Levi Enama
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace OpenEngine.Core.Scripting
{
    public class ScriptCompiler
    {
        private List<string> _references;
        private List<string> _sourceFiles;
        private CompilerParameters _compilerParameters;

        public ScriptCompiler()
        {
            _references = new List<string>
            {
                "System.dll",
                "System.Core.dll",
                "Microsoft.CSharp.dll",
                "mscorlib.dll"
            };

            _sourceFiles = new List<string>();
            _compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                TreatWarningsAsErrors = false
            };

            // Add OpenEngine.Core reference
            var coreAssembly = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(coreAssembly))
            {
                _references.Add(coreAssembly);
            }
        }

        public void AddReference(string referencePath)
        {
            if (!string.IsNullOrEmpty(referencePath) && !_references.Contains(referencePath))
            {
                _references.Add(referencePath);
            }
        }

        public void AddSourceFile(string sourceFilePath)
        {
            if (File.Exists(sourceFilePath) && !_sourceFiles.Contains(sourceFilePath))
            {
                _sourceFiles.Add(sourceFilePath);
            }
        }

        public void AddSourceCode(string sourceCode, string tempFileName = "temp_script.cs")
        {
            var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);
            File.WriteAllText(tempPath, sourceCode);
            _sourceFiles.Add(tempPath);
        }

        public CompilationResult Compile()
        {
            var provider = new CSharpCodeProvider();
            _compilerParameters.ReferencedAssemblies.Clear();
            _compilerParameters.ReferencedAssemblies.AddRange(_references.ToArray());

            var results = provider.CompileAssemblyFromFile(_compilerParameters, _sourceFiles.ToArray());

            var compilationResult = new CompilationResult
            {
                Success = results.Errors.Count == 0,
                CompiledAssembly = results.CompiledAssembly,
                Errors = new List<string>()
            };

            foreach (CompilerError error in results.Errors)
            {
                compilationResult.Errors.Add($"Line {error.Line}: {error.ErrorText}");
            }

            return compilationResult;
        }

        public Type CreateType(string sourceCode, string className, string namespaceName = "")
        {
            var tempFileName = $"{className}_temp.cs";
            AddSourceCode(sourceCode, tempFileName);

            var result = Compile();

            if (result.Success && result.CompiledAssembly != null)
            {
                var fullTypeName = string.IsNullOrEmpty(namespaceName) 
                    ? className 
                    : $"{namespaceName}.{className}";

                var type = result.CompiledAssembly.GetType(fullTypeName);
                
                // Clean up temp file
                var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return type;
            }

            return null;
        }

        public object CreateInstance(string sourceCode, string className, string namespaceName = "")
        {
            var type = CreateType(sourceCode, className, namespaceName);
            return type != null ? Activator.CreateInstance(type) : null;
        }

        public void ClearSourceFiles()
        {
            _sourceFiles.Clear();
        }
    }

    public class CompilationResult
    {
        public bool Success { get; set; }
        public Assembly CompiledAssembly { get; set; }
        public List<string> Errors { get; set; }
    }

    public class HotReloadManager
    {
        private ScriptCompiler _compiler;
        private Dictionary<string, Type> _loadedScripts;
        private Dictionary<string, DateTime> _fileWatchTimes;

        public HotReloadManager()
        {
            _compiler = new ScriptCompiler();
            _loadedScripts = new Dictionary<string, Type>();
            _fileWatchTimes = new Dictionary<string, DateTime>();
        }

        public void WatchScript(string filePath)
        {
            if (File.Exists(filePath))
            {
                _fileWatchTimes[filePath] = File.GetLastWriteTime(filePath);
            }
        }

        public void CheckForChanges()
        {
            foreach (var filePath in _fileWatchTimes.Keys)
            {
                if (File.Exists(filePath))
                {
                    var lastWrite = File.GetLastWriteTime(filePath);
                    if (lastWrite > _fileWatchTimes[filePath])
                    {
                        ReloadScript(filePath);
                        _fileWatchTimes[filePath] = lastWrite;
                    }
                }
            }
        }

        private void ReloadScript(string filePath)
        {
            var sourceCode = File.ReadAllText(filePath);
            _compiler.AddSourceCode(sourceCode);
            var result = _compiler.Compile();

            if (result.Success)
            {
                // Update loaded scripts
                Console.WriteLine($"Hot-reloaded script: {filePath}");
            }
        }
    }
}
