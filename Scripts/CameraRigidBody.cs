using Godot;
using System;

public class CameraRigidBody : RigidBody2D
{
    private float _speed = 300;
    private float _damping = 7f;

    [Signal]
    public delegate void Moved();

    public override void _Ready()
    {
        GravityScale = 0;
        LinearDamp = _damping;
    }

    public override void _Process(float delta)
    {
        if (!Freeze)
            InputProcess();   
    }

    private void InputProcess()
    {
        Vector2 direction = new Vector2(0, 0);
        if (Input.IsActionPressed("ui_up"))
            direction.y -= 1;
        if (Input.IsActionPressed("ui_left"))
            direction.x -= 1;
        if (Input.IsActionPressed("ui_down"))
            direction.y += 1;
        if (Input.IsActionPressed("ui_right"))
            direction.x += 1;
        
        if (direction != Vector2.Zero)
        {
            LinearVelocity = direction * _speed;
            EmitSignal(nameof(Moved));
        }
    }

    public bool Freeze = false;
    
}
