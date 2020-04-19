using Godot;
using System;
using static Godot.Viewport;

public class Main : Node2D
{
    private IslandGenerator _islandGenerator = new IslandGenerator();
    private IslandSpawner _islandSpawner;

    public Main()
    {
        //VisualServer.SetDebugGenerateWireframes(true);
        
    }

    public override void _Ready()
    {
        var firstIsland = _islandGenerator.Generate();
        firstIsland.Position = Vector2.Zero;
        firstIsland.SetAsFirst();
        AddChild(firstIsland);
        
        _islandSpawner = new IslandSpawner();
        AddChild(_islandSpawner);
        _islandSpawner.Connect("SpawnIsland", this, nameof(OnIslandSpawned));
        //GetViewport().DebugDraw =  (DebugDrawEnum)((((int)(GetViewport().DebugDraw) + 1 ) % 2) * 3);
    }

    public override void _Process(float delta)
    {
        
    }

    private void OnIslandSpawned(Vector2 pos)
    {
        var newIsland = _islandGenerator.Generate();
        newIsland.Position = pos;
        AddChild(newIsland);
    }
}
