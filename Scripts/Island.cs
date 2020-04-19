using Godot;
using System;

public class Island : RigidBody2D
{
    private CollisionPolygon2D _collisionPolygon;
    private Polygon2D _polygon;
    private Vector2 _centroid;
    private Arrow _arrow;
    
    public Island()
    {
        GravityScale = 0;
        LinearDamp = 0.9f;
        AngularDamp = 1.2f;
        Bounce = 1;
        _collisionPolygon = new CollisionPolygon2D();
        _polygon = new Polygon2D();

        var packedArrow = (PackedScene)ResourceLoader.Load("res://Scene/ArrowSprite.tscn");
        _arrow = (Arrow)packedArrow.Instance();
    }

    public void SetPolygon(IslandPolygon islandPolygon)
    {
        _collisionPolygon.Polygon = islandPolygon.Polygon;
        _centroid = islandPolygon.Centroid;
        _polygon.Polygon = islandPolygon.Polygon;
        _polygon.Color = Color.Color8(220, 220, 220);
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
            var mousePos = GetGlobalMousePosition();
            if (Math.Abs(Position.x - mousePos.x) > 100 || Math.Abs(Position.y - mousePos.y) > 100)
                return;
            var spaceState = GetWorld2d().DirectSpaceState;
            var result = spaceState.IntersectRay(GetGlobalMousePosition(), new Vector2(100000000, 100000000));
            if (result.Contains("collider") && result["collider"] == this)
            {
                GetNode("/root").AddChild(_arrow);
                _isSelected = true;
            }
        }
        else if (Input.IsActionJustReleased("mouse_click_left") && _isSelected == true)
        {
            GetNode("/root").RemoveChild(_arrow);
            _isSelected = false;
            ApplySlingshoot();
        }
    }

    private void ApplySlingshoot()
    {
        var powerFactor = 2;
        var mousePos = GetGlobalMousePosition();

        var shootVec = new Vector2(Position.x - mousePos.x, Position.y - mousePos.y) * powerFactor;
        ApplyImpulse(Vector2.Zero, shootVec);
        GD.Print(shootVec);
    }

    private void SlingshootDisplay()
    {
        if (!_isSelected)
        {
            return;
        }
        var mousePos = GetGlobalMousePosition();
        var shootVec = new Vector2(Position.x - mousePos.x, Position.y - mousePos.y);
        _arrow.Position = new Vector2(Position.x + shootVec.x, Position.y + shootVec.y);
        _arrow.Rotation = Mathf.Atan2(shootVec.y, shootVec.x) + Mathf.Pi / 2;
        GD.Print(_arrow.Position);
    }
}
