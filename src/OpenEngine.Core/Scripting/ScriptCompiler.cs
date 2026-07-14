// Created By Levi Enama
// Script Compiler using Roslyn for .NET 8
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace OpenEngine.Core.Scripting
{
    public class ScriptCompiler
    {
        private List<string> _references;
        private List<string> _sourceFiles;
        private CSharpCompilationOptions _compilerOptions;

        public ScriptCompiler()
        {
            _references = new List<string>
            {
                typeof(object).Assembly.Location,
                typeof(System.Linq.Enumerable).Assembly.Location,
                typeof(System.Collections.Generic.List<>).Assembly.Location
            };

            _sourceFiles = new List<string>();
            _compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithAllowUnsafe(true);

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
            var syntaxTrees = new List<SyntaxTree>();
            
            foreach (var sourceFile in _sourceFiles)
            {
                if (File.Exists(sourceFile))
                {
                    var sourceCode = File.ReadAllText(sourceFile);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceCode));
                }
            }

            var metadataReferences = _references
                .Where(r => File.Exists(r))
                .Select(r => MetadataReference.CreateFromFile(r))
                .ToList();

            var compilation = CSharpCompilation.Create(
                "DynamicScriptAssembly",
                syntaxTrees,
                metadataReferences,
                _compilerOptions
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            var compilationResult = new CompilationResult
            {
                Success = result.Success,
                Errors = new List<string>()
            };

            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                compilationResult.CompiledAssembly = Assembly.Load(ms.ToArray());
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        compilationResult.Errors.Add($"Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}");
                    }
                }
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
