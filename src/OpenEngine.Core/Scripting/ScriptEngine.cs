// Created By Levi Enama
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.IO;
using System.Linq;

namespace OpenEngine.Core.Scripting;

public class ScriptEngine
{
    private readonly List<IScript> _scripts = new();
    private readonly List<MetadataReference> _references = new();

    public ScriptEngine()
    {
        LoadReferences();
    }

    private void LoadReferences()
    {
        var coreAssembly = typeof(object).Assembly.Location;
        var systemRuntime = Assembly.Load("System.Runtime").Location;
        var systemLinq = Assembly.Load("System.Linq").Location;
        var systemCollections = Assembly.Load("System.Collections").Location;
        var thisAssembly = Assembly.GetExecutingAssembly().Location;

        _references.Add(MetadataReference.CreateFromFile(coreAssembly));
        _references.Add(MetadataReference.CreateFromFile(systemRuntime));
        _references.Add(MetadataReference.CreateFromFile(systemLinq));
        _references.Add(MetadataReference.CreateFromFile(systemCollections));
        _references.Add(MetadataReference.CreateFromFile(thisAssembly));
    }

    public IScript CompileAndLoad(string code, string className = "DynamicScript")
    {
        string fullCode = $@"
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

public class {className} : IScript
{{
    public SimEntity Entity {{ get; set; }}
    public void OnStart() {{ }}
    public void OnUpdate(float deltaTime) {{ }}
    {code}
}}";

        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);
        var compilation = CSharpCompilation.Create(
            $"Script_{Guid.NewGuid():N}",
            new[] { syntaxTree },
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);
        
        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            throw new InvalidOperationException($"Compilation failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        var type = assembly.GetType(className);
        if (type == null) throw new InvalidOperationException("Script class not found.");

        var script = (IScript)Activator.CreateInstance(type)!;
        _scripts.Add(script);
        return script;
    }

    public void UpdateAll(float deltaTime)
    {
        foreach (var script in _scripts)
            script.OnUpdate(deltaTime);
    }
}

public interface IScript
{
    SimEntity Entity { get; set; }
    void OnStart();
    void OnUpdate(float deltaTime);
}
