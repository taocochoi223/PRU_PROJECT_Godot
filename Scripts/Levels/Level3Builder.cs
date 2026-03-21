using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Level 3 Builder: The Dark Citadel / Boss Arena
/// Hybrid of Forest and Cave styles, leading to a climatic climatic arena.
/// </summary>
public partial class Level3Builder : Node2D
{
    // Map bounds (Slightly shorter but wider for the arena)
    private const float MAP_WIDTH = 4500f;
    private const float MAP_HEIGHT = 1000f;

    // Palette - A mix of dark forest and purple/shadow citadel
    private static readonly Color CitadelBack = new Color(0.05f, 0.02f, 0.08f);
    private static readonly Color CitadelGround = new Color(0.12f, 0.08f, 0.15f);
    private static readonly Color PathColor = new Color(0.18f, 0.12f, 0.22f);
    private static readonly Color ArenaColor = new Color(0.25f, 0.15f, 0.30f);

    // Resources
    private Texture2D _texRock = GD.Load<Texture2D>("res://Assets/Sprites/Environment/rock_pixel.png");
    private Texture2D _texAncientTree = GD.Load<Texture2D>("res://Assets/Sprites/Environment/tree_pixel.png");
    private PackedScene _snakeScene = GD.Load<PackedScene>("res://Scenes/Enemies/IsometricSnake.tscn");
    private PackedScene _eagleScene = GD.Load<PackedScene>("res://Scenes/Enemies/Eagle.tscn");
    private PackedScene _bossScene = GD.Load<PackedScene>("res://Scenes/Enemies/ChanTinh.tscn");
    private PackedScene _princessScene = GD.Load<PackedScene>("res://Scenes/NPCs/Princess.tscn");
    private PackedScene _cageScene = GD.Load<PackedScene>("res://Scenes/Objects/BossCage.tscn");

    private Random _rng = new Random(333); // Level 3 seed

    public override void _Ready()
    {
        YSortEnabled = true;

        BuildBackground();
        BuildPaths();
        BuildAtmosphere();
        BuildDecorations();
        BuildArenaStructures();
        BuildEnemies();
        SpawnTargetNPCs();
    }

    private void BuildBackground()
    {
        // Dark Void Background
        var bg = new ColorRect {
            ZIndex = -200,
            Position = new Vector2(-1000, -1000),
            Size = new Vector2(MAP_WIDTH + 2000, MAP_HEIGHT + 2000),
            Color = CitadelBack
        };
        AddChild(bg);

        // Ground Floor
        var ground = new ColorRect {
            ZIndex = -190,
            Size = new Vector2(MAP_WIDTH, MAP_HEIGHT),
            Color = CitadelGround
        };
        AddChild(ground);
    }

    private void BuildPaths()
    {
        // Path leading to the arena (2500, 500)
        Vector2[] pathPoints = {
            new(100, 500), new(500, 450), new(1000, 550), new(1500, 500), new(2000, 450), new(2500, 500)
        };

        var path = new Line2D {
            ZIndex = -180,
            Points = pathPoints,
            DefaultColor = PathColor,
            Width = 120f,
            BeginCapMode = Line2D.LineCapMode.Round,
            EndCapMode = Line2D.LineCapMode.Round,
            JointMode = Line2D.LineJointMode.Round
        };
        AddChild(path);
        
        // Large Circular Arena Floor at the end
        var arenaFloor = new Polygon2D {
            ZIndex = -185,
            Color = ArenaColor,
            Position = new Vector2(3000, 500),
            Polygon = MakeEllipsePolygon(800, 400, 32)
        };
        AddChild(arenaFloor);
    }

    private void BuildAtmosphere()
    {
        // Purple Fog layers
        for (int i = 0; i < 6; i++)
        {
            var fog = new ColorRect {
                ZIndex = 50,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Size = new Vector2(800 + (float)_rng.NextDouble() * 400, 100 + (float)_rng.NextDouble() * 50),
                Position = new Vector2(i * 700f, (float)_rng.NextDouble() * MAP_HEIGHT),
                Color = new Color(0.2f, 0.05f, 0.3f, 0.15f),
                Name = $"Fog_{i}"
            };
            AddChild(fog);
        }

        // Fireflies (Citadel wisps)
        for (int i = 0; i < 15; i++)
        {
            var fly = new ColorRect {
                ZIndex = 40,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Size = new Vector2(4, 4),
                Position = new Vector2((float)_rng.NextDouble() * MAP_WIDTH, (float)_rng.NextDouble() * MAP_HEIGHT),
                Color = new Color(1f, 0.2f, 0.1f, 0.8f),
                Name = $"Firefly_{i}"
            };
            AddChild(fly);
        }
    }

    private void BuildDecorations()
    {
        // Dead Trees and Sharp Rocks along the way
        for (int i = 0; i < 40; i++)
        {
            float x = (float)_rng.NextDouble() * 2200f; // Only before arena
            float y = (float)_rng.NextDouble() * MAP_HEIGHT;
            
            // Skip paths
            if (y > 400 && y < 600) continue;

            if (_rng.NextDouble() > 0.6)
            {
                CreateStaticSprite(_texAncientTree, new Vector2(x, y), 0.8f + (float)_rng.NextDouble() * 0.4f, new Color(0.3f, 0.2f, 0.4f));
            }
            else
            {
                CreateStaticSprite(_texRock, new Vector2(x, y), 0.5f + (float)_rng.NextDouble() * 0.5f, new Color(0.4f, 0.35f, 0.5f));
            }
        }
    }

    private void BuildArenaStructures()
    {
        // Pillars around the arena
        Vector2 center = new Vector2(3000, 500);
        float rx = 850f;
        float ry = 450f;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * (Mathf.Pi * 2 / 8);
            Vector2 pos = center + new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
            
            // Use Rock as pillar base
            CreateStaticSprite(_texRock, pos, 1.5f, new Color(0.2f, 0.15f, 0.25f));
        }
    }

    private void BuildEnemies()
    {
        // Patrol Snakes on the path
        Vector2[] snakePos = { new(800, 500), new(1600, 480), new(2200, 520) };
        foreach (var pos in snakePos)
        {
            var snake = _snakeScene.Instantiate<Node2D>();
            snake.GlobalPosition = pos;
            AddChild(snake);
        }

        // Eagles guarding the approach
        Vector2[] eaglePos = { new(1200, 300), new(1800, 700) };
        foreach (var pos in eaglePos)
        {
            var eagle = _eagleScene.Instantiate<Node2D>();
            eagle.GlobalPosition = pos;
            AddChild(eagle);
        }

        // The Boss
        var boss = _bossScene.Instantiate<Node2D>();
        boss.Name = "ChanTinh";
        boss.GlobalPosition = new Vector2(3300, 520);
        AddChild(boss);
    }

    private void SpawnTargetNPCs()
    {
        // Princess in Cage
        var cage = _cageScene.Instantiate<Node2D>();
        cage.Name = "BossCage";
        cage.GlobalPosition = new Vector2(3500, 480); 
        AddChild(cage);

        var princess = _princessScene.Instantiate<Node2D>();
        princess.Name = "Princess";
        princess.GlobalPosition = new Vector2(3500, 482); // Slightly below cage for Y-sorting
        princess.ZIndex = 2; // Ensure she is above the cage background (-1) but behind bars (5)
        AddChild(princess);
    }

    private void CreateStaticSprite(Texture2D tex, Vector2 pos, float scale, Color modulate)
    {
        var sprite = new Sprite2D {
            Texture = tex,
            Position = pos,
            Scale = new Vector2(scale, scale),
            Modulate = modulate,
            YSortEnabled = true,
            Offset = new Vector2(0, -tex.GetSize().Y / 2)
        };
        
        // Disabled collision for obstacles in Level 3 as per user request
        
        AddChild(sprite);
    }

    private Vector2[] MakeEllipsePolygon(float rx, float ry, int points)
    {
        var poly = new Vector2[points];
        for (int i = 0; i < points; i++)
        {
            float a = i * (Mathf.Pi * 2 / points);
            poly[i] = new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
        }
        return poly;
    }
}
