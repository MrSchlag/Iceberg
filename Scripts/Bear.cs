using System;

using Godot;
public class Bear : KinematicBody2D
{
    public bool IsJumping;
    public bool IsTurning;
    private int _turnSide;
    private Island _targetIsland;
    public Island ActualIsland;
    private AnimatedSprite _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _animatedSprite.Frame = 12;
    }

    public override void _Process(float delta)
    {
        if (_animatedSprite.Frame == 12)
        {
            _animatedSprite.Stop();
            _animatedSprite.Frame = 0;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        ContinueJump(delta);
    }

    public void ContinueJump(float delta)
    {
        if (IsJumping)
        {
            _animatedSprite.Play();
            
            if (Math.Abs(_targetIsland.GlobalPosition.x - GlobalPosition.x) < 2 && Math.Abs(_targetIsland.GlobalPosition.y - GlobalPosition.y) < 2)
            {
                ActualIsland.BearLeave(this);
                Position = Vector2.Zero;
                IsJumping = false;
                _animatedSprite.Stop();
                _animatedSprite.Frame = 0;
                _targetIsland.TakeBear(this);
                return;
            }
            
            MoveAndSlide((_targetIsland.GlobalPosition - GlobalPosition).Normalized() * 100);
        }
    }

    public void ContinueTurn(float delta)
    {
    }

    public void JumpTo(Island island)
    {
        _targetIsland = island;
        IsJumping = true;
    }
}
