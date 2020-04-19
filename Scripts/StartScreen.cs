using Godot;
using System;

public class StartScreen : Node2D
{
    Polygon2D _polygon2d;
    Polygon2D _polygon2d1;
    Polygon2D _polygon2d2;
    
    public override void _Ready()
    {
        _polygon2d = GetNode<Polygon2D>("Polygon2D/Polygon2D");
        _polygon2d1 = GetNode<Polygon2D>("Polygon2D/Polygon2D2");
        _polygon2d2 = GetNode<Polygon2D>("Polygon2D/Polygon2D3");
    }
    
    float ClickCounter = 0;
    public override void _Process(float delta)
    {
        ClickCounter += delta;
        _polygon2d.RotationDegrees += 10 * delta;
        _polygon2d1.RotationDegrees += 15 * delta;
        _polygon2d2.RotationDegrees += -30 * delta;

        if (Input.IsActionPressed("mouse_click_left") && ClickCounter > 2)
            GetTree().ChangeScene("res://Scene/Main.tscn");
    }
}
