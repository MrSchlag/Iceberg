using Godot;
using System;
using System.Collections.Generic;

public class IslandSpawner : Node2D
{
    private Random _rand = new Random();
    private bool _isInit = false;
    private Vector2 _startAreaDiscovered = Vector2.Zero;
    private Vector2 _endAreaDicovered = Vector2.Zero;

    private OpenSimplexNoise _noise = new OpenSimplexNoise();
    private Lazy<Camera2D> _camera;
    private Lazy<RigidBody2D> _cameraRigidBody;

    [Signal]
    public delegate void SpawnIsland(Vector2 pos);

    public override void _Ready()
    {
        _noise.Seed = DateTime.Now.Millisecond;
        _noise.Octaves = 1;
        _noise.Period = 1;
        _noise.Persistence = 1000;

        _cameraRigidBody = new Lazy<RigidBody2D>(() =>
        {
            var v = GetNode<CameraRigidBody>("/root/Node2D/CameraRigidBody");
            v.Connect("Moved", this, nameof(OnRigidBodyCameraMoved));
            return v;
        });

        _camera = new Lazy<Camera2D>(() => _cameraRigidBody.Value.GetNode<Camera2D>("Camera2D"));
    }

    public override void _Process(float delta)
    {
        var camera = _camera.Value;
        if (!_isInit)
            InitIslands();
    }

    private void InitIslands()
    {
        var threshold = 0.81f;
        var size = _camera.Value.GetViewport().Size;
        
        size.x *= _camera.Value.Zoom.x;
        size.y *= _camera.Value.Zoom.y;

        var start = new Vector2(_cameraRigidBody.Value.Position.x - (size.x / 2), _cameraRigidBody.Value.Position.y - (size.y / 2));
        var end = new Vector2(_cameraRigidBody.Value.Position.x + (size.x / 2), _cameraRigidBody.Value.Position.y + (size.y / 2));

        var step = 10f;

        for (float x = start.x; x < end.x; x += step)
        {
            for (float y = start.y; y < end.y; y += step)
            {
                if (_noise.GetNoise2d(x, y) > threshold)
                    EmitSignal(nameof(SpawnIsland), new Vector2(x, y));
            }
        }
        _isInit = true;
    }

    private void OnRigidBodyCameraMoved()
    {
        var threshold = 0.79f;
        var size = _camera.Value.GetViewport().Size;
        
        size.x *= _camera.Value.Zoom.x;
        size.y *= _camera.Value.Zoom.y;

        var start = new Vector2(_cameraRigidBody.Value.Position.x - (size.x / 2), _cameraRigidBody.Value.Position.y - (size.y / 2));
        var end = new Vector2(_cameraRigidBody.Value.Position.x + (size.x / 2), _cameraRigidBody.Value.Position.y + (size.y / 2));

        start.x = Mathf.Min(start.x, _startAreaDiscovered.x);
        start.y = Mathf.Min(start.y, _startAreaDiscovered.y);

        end.x = Mathf.Max(end.x, _endAreaDicovered.x);
        end.y = Mathf.Max(end.y, _endAreaDicovered.y);

        var step = 20;

        if (start.y < _startAreaDiscovered.y)
        {
            for (float x = start.x; x < end.x; x += step)
                if (_noise.GetNoise2d(x, start.y) > threshold)
                    EmitSignal(nameof(SpawnIsland), new Vector2(x, start.y));
            _startAreaDiscovered.y = start.y;
        }
        else if (end.y > _endAreaDicovered.y)
        {
            for (float x = start.x; x < end.x; x += step)
                if (_noise.GetNoise2d(x, end.y) > threshold)
                    EmitSignal(nameof(SpawnIsland), new Vector2(x, end.y));
            _endAreaDicovered.y = end.y;
        }

        if (start.x < _startAreaDiscovered.x)
        {
            for (float y = start.y; y < end.y; y += step)
                if (_noise.GetNoise2d(start.x, y) > threshold)
                    EmitSignal(nameof(SpawnIsland), new Vector2(start.x, y));
            _startAreaDiscovered.x = start.x;
        }
        else if (end.x > _endAreaDicovered.x)
        {
            for (float y = start.y; y < end.y; y += step)
                if (_noise.GetNoise2d(end.x, y) > threshold)
                    EmitSignal(nameof(SpawnIsland), new Vector2(end.x, y));
            _endAreaDicovered.x = end.x;
        }
    }
}