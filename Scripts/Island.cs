using Godot;
using System;

public class Island : RigidBody2D
{
    private CollisionPolygon2D _collisionPolygon;
    private Polygon2D _polygon;

    public Island(  )
    {
        GravityScale = 0;
        LinearDamp = 0.9f;
        Bounce = 1;
        _collisionPolygon = new CollisionPolygon2D();
        _polygon = new Polygon2D();
    }

    public void SetPolygon(Vector2[] polygonArray)
    {
        _collisionPolygon.Polygon = polygonArray;
        
        _polygon.Polygon = polygonArray;
        _polygon.Color = Color.Color8(255, 255, 255);
    }

    public override void _Ready()
    {
        AddChild(_collisionPolygon);
        AddChild(_polygon);
    }

    public override void _Process(float delta)
    {
        MouseInputProcess();
        SlingshootDisplay();
    }

    private bool _isSelected = false;
    private void MouseInputProcess()
    {
        if (Input.IsActionJustPressed("mouse_click_left") && _isSelected == false)
        {
            var spaceState = GetWorld2d().DirectSpaceState;
            var result = spaceState.IntersectRay(GetGlobalMousePosition(), new Vector2(100000000, 100000000));
            if (result.Contains("collider") && result["collider"] == this)
            {
                _isSelected = true;
            }
        }
        else if (Input.IsActionJustReleased("mouse_click_left") && _isSelected == true)
        {
            _isSelected = false;
            ApplySlingshoot();
        }
    }

    private void ApplySlingshoot()
    {
        var powerFactor = 2;
        var mousePos = GetGlobalMousePosition();

        var shootVec = new Vector2(Position.x - mousePos.x, Position.y - mousePos.y) * powerFactor;
        LinearVelocity = shootVec;
        GD.Print(shootVec);
    }

    private void SlingshootDisplay()
    {

    }
}
