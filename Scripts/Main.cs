using Godot;
using System;

public class Main : Node2D
{
    private IslandGenerator _islandGenerator = new IslandGenerator();
    private IslandSpawner _islandSpawner;
    public override void _Ready()
    {
        

        _islandSpawner = new IslandSpawner();
        AddChild(_islandSpawner);
        _islandSpawner.Connect("SpawnIsland", this, nameof(OnIslandSpawned));
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
