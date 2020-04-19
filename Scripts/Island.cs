using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Island : RigidBody2D
{

    Gradient _gradient;
    private CollisionPolygon2D _collisionPolygon;
    private Polygon2D _polygon;
    private Vector2 _centroid;
    private Arrow _arrow;
    private Bear _bear = null;
    public bool _alreadyUsed = false;
    
    public Island()
    {
        GravityScale = 0;
        LinearDamp = 0.9f;
        AngularDamp = 1.2f;
        Bounce = 0.05f;

        RemainingTime = (float)new Random().Next(9, 16);
        RemainingTime = RemainingTime > 12f ? 12f : RemainingTime;
        _collisionPolygon = new CollisionPolygon2D();
        _polygon = new Polygon2D();

        _gradient = (Gradient)ResourceLoader.Load("res://IceGradient.tres");

        var packedArrow = (PackedScene)ResourceLoader.Load("res://Scene/ArrowSprite.tscn");
        _arrow = (Arrow)packedArrow.Instance();
    }

    public void SetAsFirst()
    {
        var packedBear = (PackedScene)ResourceLoader.Load("res://Scene/Bear.tscn");
        _bear = (Bear)packedBear.Instance();
        AddChild(_bear);
    }

    public void TakeBear(Bear bear)
    {
        _bear = bear;
        AddChild(_bear);
    }

    public void SetPolygon(IslandPolygon islandPolygon)
    {
        _collisionPolygon.Polygon = islandPolygon.Polygon;
        _centroid = islandPolygon.Centroid;
        _polygon.Polygon = islandPolygon.Polygon;
        _polygon.Color = _gradient.Interpolate(RemainingTime / MaxRemainingTime);
    }

    public override void _Ready()
    {
        AddChild(_collisionPolygon);
        AddChild(_polygon);
    }

    public override void _Process(float delta)
    {
        //Update();
        SinkProcess(delta);
        MouseInputProcess();
        SlingshootDisplay();
        ScanOtherIsland(delta);
        GiveBear();
    }

    public const float MaxRemainingTime = 12;
    public float RemainingTime;
    public bool StartedToSink = false;
    private void SinkProcess(float delta)
    {
        if (_bear != null || StartedToSink)
        {
            RemainingTime -= delta;
            StartedToSink = true;
            _polygon.Color = _gradient.Interpolate(RemainingTime / MaxRemainingTime);
        }
    }

    private bool _isSelected = false;
    private void MouseInputProcess()
    {
        if (Input.IsActionJustPressed("mouse_click_left") && !_isSelected && _bear == null)
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
    }

    private void ScanOtherIsland(float delta)
    {
        if (_bear == null)
            return;

        var spaceState = GetWorld2d().DirectSpaceState;
        _closeIsland.ForEach(i => i.Updated = false);
        foreach (var corner in _collisionPolygon.Polygon)
        {
            foreach (var rad in RadianCircleProvider.RadianCircle)
            {
                var cornerPos = new Vector2(corner.x + Position.x, corner.y + Position.y);
                var angleVec = new Vector2(Mathf.Cos(rad) * 50, Mathf.Sin(rad) * 50);
                
                var destPoint = new Vector2(angleVec.x + cornerPos.x, angleVec.y + cornerPos.y);
                
                var result = spaceState.IntersectRay(cornerPos, destPoint, new Godot.Collections.Array(new object [] {(RigidBody2D)this}));

                if (result.Count > 0)
                {
                    var contactPos = (Vector2)result["position"];
                    var dist = Mathf.Sqrt(Mathf.Pow(contactPos.x - cornerPos.x, 2) + Mathf.Pow(contactPos.y - cornerPos.y, 2));
                    if (dist < 20)
                    {
                        GD.Print("under 10");
                        var island = _closeIsland.SingleOrDefault(i => i.Island == result["collider"] && ((Island)result["collider"])._alreadyUsed == false);
                        
                        if (island == null)
                        {
                            GD.Print("adder to closeIslands");
                            _closeIsland.Add(new IslandTimeClose() {Time = 0, Island = (Island)result["collider"], Updated = true});
                        }
                        else if (island.Updated == false)
                        {
                            island.Time += delta;
                            island.Updated = true;
                            GD.Print("updated in closeIslands delta " + island.Time);
                        }
                    }
                }
            }
        }
        _closeIsland.RemoveAll(i => !i.Updated);
    }

    private class IslandTimeClose
    {
        public bool Updated;
        public float Time {get; set;}
        public Island Island { get; set;}
    }
    private List<IslandTimeClose> _closeIsland = new List<IslandTimeClose>();

    private void GiveBear()
    {
        if (_bear == null)
            return;
        var newIsland = _closeIsland.FirstOrDefault(i => i.Time > 2);
        if (newIsland != null)
        {
            GD.Print("giveBear");
            RemoveChild(_bear);
            newIsland.Island.TakeBear(_bear);
            _bear = null;
            _alreadyUsed = true;
        }
    }
}
