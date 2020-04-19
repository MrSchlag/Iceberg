using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Island : RigidBody2D
{
    private Random _rand = new Random();
    Gradient _gradient;
    private CollisionPolygon2D _collisionPolygon;
    private Polygon2D _polygon;
    private Vector2 _centroid;
    private Arrow _arrow;
    private Bear _bear = null;
    public bool AlreadyUsed = false;
    
    public Island()
    {
        GravityScale = 0;
        LinearDamp = 0.9f;
        AngularDamp = 1.2f;
        Bounce = 0.05f;
        AngularDamp = 0.5f;

        RemainingTime = (float)new Random().Next(6, 15);
        RemainingTime = RemainingTime > MaxRemainingTime ? MaxRemainingTime : RemainingTime;
        _collisionPolygon = new CollisionPolygon2D();
        _polygon = new Polygon2D();

        _gradient = (Gradient)ResourceLoader.Load("res://IceGradient.tres");

        var packedArrow = (PackedScene)ResourceLoader.Load("res://Scene/ArrowSprite.tscn");
        _arrow = (Arrow)packedArrow.Instance();
    }
    
    public void BearLeave(Bear bear)
    {
        if (_bear == bear)
        {
            RemoveChild(bear);
            _bear = null;
        }
    }

    public void SetAsFirst()
    {
        var packedBear = (PackedScene)ResourceLoader.Load("res://Scene/Bear.tscn");
        _bear = (Bear)packedBear.Instance();
        _bear.ActualIsland = this;
        AddChild(_bear);
    }

    public void TakeBear(Bear bear)
    {
        _bear = bear;
        _bear.ActualIsland = this;
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
        SinkProcess(delta);
        MouseInputProcess();
        SlingshootDisplay();
        ScanOtherIsland(delta);
        GiveBear();
    }

    public const float MaxRemainingTime = 8;
    public float RemainingTime;
    public bool StartedToSink = false;
    private void SinkProcess(float delta)
    {
        if (_bear != null || StartedToSink)
        {
            StartedToSink = true;
            RemainingTime -= delta * 1;
            if (RemainingTime < 0 && _bear == null)
                QueueFree();
            else if (RemainingTime < 0 && _bear.IsJumping == false)
            {
                GD.Print("sunk");
                EmitSignal(nameof(SunkWithBear));
                
                var childs = GetChildren();
                for (int i = 0; i < childs.Count; i++)
                {
                    if (childs[i] == _bear)
                    {
                        RemoveChild(_bear);
                    }
                }
                QueueFree();
            }
        }
        else
        {
            RemainingTime -= delta * _forceSinkFactor;
            if (RemainingTime < 0)
                QueueFree();

        }
        _polygon.Color = _gradient.Interpolate(RemainingTime / MaxRemainingTime);
    }

    private float _forceSinkFactor = 0.02f;
    public void ForceSink()
    {
        StartedToSink = true;
        _forceSinkFactor = 13f;
    }

    [Signal]
    public delegate void SunkWithBear();

    private bool _isSelected = false;
    private void MouseInputProcess()
    {
        if (Input.IsActionJustPressed("mouse_click_left") && !_isSelected && _bear == null && !AlreadyUsed)
        {
            var mousePos = GetGlobalMousePosition();
            if (Math.Abs(Position.x - mousePos.x) > 100 || Math.Abs(Position.y - mousePos.y) > 100)
                return;
            var spaceState = GetWorld2d().DirectSpaceState;
            var result = spaceState.IntersectRay(GetGlobalMousePosition(), new Vector2(100000000, 100000000));
            if (result.Contains("collider") && result["collider"] == this)
            {
                GetNode<AudioStreamPlayer2D>("/root/Node2D/CameraRigidBody/AudioStreamSelect" + _rand.Next(1, 5)).Play();
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
        if (_bear == null || _bear.IsJumping)
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
                        var island = _closeIsland.SingleOrDefault(i => i.Island == result["collider"] && ((Island)result["collider"]).AlreadyUsed == false);
                        
                        if (island == null)
                        {
                            _closeIsland.Add(new IslandTimeClose() {Time = 0, Island = (Island)result["collider"], Updated = true});
                        }
                        else if (island.Updated == false)
                        {
                            island.Time += delta;
                            island.Updated = true;
                        }
                    }
                }
            }
        }
        _closeIsland.RemoveAll(i => i.Updated == false || i.Island.AlreadyUsed);
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
        if (_bear == null || _bear.IsJumping)
            return;
        var newIsland = _closeIsland.FirstOrDefault(i => i.Time > 2);
        if (newIsland != null)
        {
            _bear.JumpTo(newIsland.Island);
            AlreadyUsed = true;
        }
        else if (_closeIsland.Any(i => i.Updated && !i.Island.AlreadyUsed))
        {
            var island = _closeIsland.Where(i => i.Updated && !i.Island.AlreadyUsed).OrderBy(i => i.Time).Last();
            _bear.LookAt(island.Island.Position);
        }
    }
}
