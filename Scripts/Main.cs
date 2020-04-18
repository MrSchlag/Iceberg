using Godot;
using System;

public class Main : Node2D
{
    private IslandGenerator _islandGenerator = new IslandGenerator();

    public override void _Ready()
    {
        var rand = new Random();
        for (int i = 0; i < 20; i++)
        {
            var node = _islandGenerator.Generate();
            node.Position = new Vector2(rand.Next(0, 300), rand.Next(0, 200));
            AddChild(node);
        }
        
    }

    public override void _Process(float delta)
    {
        
    }
}
