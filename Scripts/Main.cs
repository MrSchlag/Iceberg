using Godot;
using System;
using System.Collections.Generic;
using static Godot.Viewport;

public class Main : Node2D
{
    private IslandGenerator _islandGenerator = new IslandGenerator();
    private IslandSpawner _islandSpawner;
    private List<Island> _allIslands = new List<Island>();

    private float _timerScore = 0;
    private RichTextLabel _scroreLabel;

    public Main()
    {
        //VisualServer.SetDebugGenerateWireframes(true);
        
    }

    public override void _Ready()
    {
        var firstIsland = _islandGenerator.Generate();
        firstIsland.Position = Vector2.Zero;
        firstIsland.SetAsFirst();
        firstIsland.Connect("SunkWithBear", this, nameof(OnEndGame));
        AddChild(firstIsland);
        
        _islandSpawner = new IslandSpawner();
        AddChild(_islandSpawner);
        _islandSpawner.Connect("SpawnIsland", this, nameof(OnIslandSpawned));
        //GetViewport().DebugDraw =  (DebugDrawEnum)((((int)(GetViewport().DebugDraw) + 1 ) % 2) * 3);
        GetNode<AudioStreamPlayer2D>("CameraRigidBody/AudioStreamPlayer2D").Play();
        GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel").Visible = false;
        GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel2").Visible = false;
        GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel4").Visible = false;

        _scroreLabel = GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel3");
        _scroreLabel.Text = string.Empty;

    }

    public override void _Process(float delta)
    {
        _scroreLabel.Text = ((int)_timerScore).ToString() + "°C";
        
        if (EndGame == true)
        {
            ScoreCounter += delta;
            EndCounter += delta;
            if (EndCounter > 4)
            {
                GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel2").Visible = true;
                if (Input.IsActionPressed("mouse_click_left"))
                {
                    GetTree().ChangeScene("res://Scene/StartScreen.tscn");
                }
            }
            if (ScoreCounter > 2)
            {
                GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel4").Visible = true;
                GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel4").Text = _scroreLabel.Text;
            }
        }
        else
        {
            _timerScore += delta;
        }
    }

    private void OnIslandSpawned(Vector2 pos)
    {
        var newIsland = _islandGenerator.Generate();
        newIsland.Position = pos;
        AddChild(newIsland);
        newIsland.Connect("SunkWithBear", this, nameof(OnEndGame));
        _allIslands.Add(newIsland);
    }

    public float ScoreCounter = 0;
    public float EndCounter = 0;
    public bool EndGame = false;
    private void OnEndGame()
    {
        GetNode<CameraRigidBody>("CameraRigidBody").Freeze = true;
        _allIslands.ForEach(i => i.ForceSink());
        EndGame = true;
        GetNode<RichTextLabel>("CameraRigidBody/CanvasLayer/RichTextLabel").Visible = true;


        _scroreLabel.Visible = false;
    }
}
