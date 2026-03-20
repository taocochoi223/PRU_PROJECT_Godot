using Godot;
using System;

public partial class IsometricMudPit : Area2D
{
    [Export] public float SlowMultiplier = 0.4f;
    private float _originalSpeed = 250.0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is IsometricPlayer player)
        {
            _originalSpeed = (float)player.Get("Speed");
            player.Set("Speed", _originalSpeed * SlowMultiplier);
            
            // Visual feedback: darkening the player slightly when in mud
            player.Modulate = new Color(0.7f, 0.6f, 0.5f); 
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is IsometricPlayer player)
        {
            player.Set("Speed", _originalSpeed);
            player.Modulate = Colors.White;
        }
    }
}
