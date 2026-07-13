// Created By Levi Enama
using Xunit;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Math;
using OpenEngine.Core.Intent;

namespace OpenEngine.Tests;

public class EngineTests
{
    [Fact]
    public void CreateEntity_ShouldAddToDictionary()
    {
        var engine = new OpenSimulationEngine();
        var entity = engine.CreateEntity("Test Entity", "TestType");
        
        Assert.NotNull(entity);
        Assert.Equal("Test Entity", entity.Name);
        Assert.Equal("TestType", entity.Type);
        Assert.True(engine.Entities.ContainsKey(entity.Id));
    }

    [Fact]
    public void CreateConnection_ShouldBeStored()
    {
        var engine = new OpenSimulationEngine();
        var e1 = engine.CreateEntity("A", "TypeA");
        var e2 = engine.CreateEntity("B", "TypeB");
        var conn = engine.CreateConnection(e1.Id, e2.Id, ConnectionPredicate.FRIENDLY_TO);
        
        Assert.NotNull(conn);
        Assert.Equal(e1.Id, conn.SourceId);
        Assert.Equal(e2.Id, conn.TargetId);
        Assert.Equal(ConnectionPredicate.FRIENDLY_TO, conn.Predicate);
    }

    [Fact]
    public void GetEntitiesInRadius_ShouldReturnCorrectCount()
    {
        var engine = new OpenSimulationEngine();
        var center = new Vector3(0, 0, 0);
        engine.CreateEntity("Near", "X") { Position3D = new(5, 0, 0) };
        engine.CreateEntity("Far", "X") { Position3D = new(15, 0, 0) };
        
        var inRadius = engine.GetEntitiesInRadius(center, 10);
        Assert.Single(inRadius);
    }
}

public class IntentParserTests
{
    [Fact]
    public void ParseIntent_ShouldDetectMoveAction()
    {
        var parser = new UniversalIntentParser();
        var text = "I need to move to the safe sector at coordinates (100, 50, 200).";
        var actions = parser.ParseIntent(text, "test-entity");
        
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == ActionVerb.MOVE_TO);
    }

    [Fact]
    public void FallbackRegex_ShouldParseMove()
    {
        var parser = new UniversalIntentParser();
        var text = "Let's MOVE towards the station!";
        var action = parser.FallbackRegexParse(text, "test-entity");
        
        Assert.NotNull(action);
        Assert.Equal(ActionVerb.MOVE_TO, action.Value.Action);
    }
}

public class Vector3Tests
{
    [Fact]
    public void Distance_ShouldCalculateCorrectly()
    {
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(3, 0, 4);
        float distance = Vector3.Distance(a, b);
        
        Assert.Equal(5.0f, distance, precision: 3);
    }

    [Fact]
    public void Add_ShouldWorkCorrectly()
    {
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(4, 5, 6);
        var result = a + b;
        
        Assert.Equal(new Vector3(5, 7, 9), result);
    }
}
