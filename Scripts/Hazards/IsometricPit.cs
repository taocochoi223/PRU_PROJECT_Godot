using Godot;
using System;

public partial class IsometricPit : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        // Collision layer should be set to detect player (layer 1)
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is IsometricPlayer player)
        {
            // Call the fall logic on player
            player.FallIntoPit();
        }
    }

    public override void _Process(double delta)
    {
        // Continuous check for bodies inside that might have just landed at Z=0
        foreach (var body in GetOverlappingBodies())
        {
            if (body is IsometricPlayer player && player.Z <= 5.0f)
            {
                player.FallIntoPit();
            }
        }
    }
}
