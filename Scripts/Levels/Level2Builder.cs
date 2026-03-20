using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Level 2 Builder: Dark Cave Environment
/// Pro-level environment with occlusion, dark atmosphere, falling rocks, and hidden traps.
/// </summary>
public partial class Level2Builder : Node2D
{
    // Constants & Palette
    private const float MAP_WIDTH = 4000f;
    private const float MAP_HEIGHT = 1200f;
    private static readonly Color DarkCaveColor = new Color("#454555"); // Brighter cave tint
    private static readonly Color StonyGray = new Color(0.35f, 0.35f, 0.38f);
    private static readonly Color PathColor = new Color(0.15f, 0.12f, 0.1f, 0.6f); // Dark trail

    // Resources
    private Shader _caveStoneShader = GD.Load<Shader>("res://Assets/Shaders/cave_stone.gdshader");
    private Texture2D _rockTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/rock_pixel.png");
    private Texture2D _grassTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/grass.png");
    private Texture2D _floorTexture = GD.Load<Texture2D>("res://Assets/Sprites/Backgrounds/cave_ground.png");
    private PackedScene _exitScene = GD.Load<PackedScene>("res://Scenes/Objects/LevelExit.tscn");
    private PackedScene _spikeTrapScene = GD.Load<PackedScene>("res://Scenes/Hazards/SpikeHazard.tscn");

    private Random _rng = new Random(42);
    private Timer _rockTimer;

    public override void _Ready()
    {
        // 1. Setup Sorting & Layers
        YSortEnabled = true;
        ZIndex = 0; // Builder root is neutral

        // 2. Build Environment
        BuildAtmosphere();
        BuildFloorAndPath();
        BuildBoundaries();
        BuildVegetation();
        BuildObstacles();
        BuildHazards();
        BuildExitPortal();
        
        // 3. Setup Dynamic Systems
        SetupFallingRocks();
    }

    private void BuildAtmosphere()
    {
        // Global darkness tint
        var canvasModulate = new CanvasModulate();
        canvasModulate.Color = DarkCaveColor;
        AddChild(canvasModulate);

        // Solid background for editor/empty areas
        var bg = new ColorRect();
        bg.Color = new Color(0.01f, 0.01f, 0.02f);
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        bg.Size = new Vector2(MAP_WIDTH + 1000, MAP_HEIGHT + 1000);
        bg.Position = new Vector2(-500, -500);
        bg.ZIndex = -200; // Far back
        AddChild(bg);
    }

    private void BuildFloorAndPath()
    {
        // Stone Base with Texture
        var floor = new Sprite2D();
        floor.Texture = _floorTexture;
        floor.RegionEnabled = true;
        floor.RegionRect = new Rect2(0, 0, MAP_WIDTH, MAP_HEIGHT);
        floor.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        floor.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
        floor.Position = new Vector2(MAP_WIDTH / 2, MAP_HEIGHT / 2);
        floor.ZIndex = -100;

        var mat = new ShaderMaterial();
        mat.Shader = _caveStoneShader;
        floor.Material = mat;
        floor.SelfModulate = StonyGray;
        AddChild(floor);

        // Visible Path (Line2D)
        var path = new Line2D();
        path.DefaultColor = PathColor;
        path.Width = 180f;
        path.ZIndex = -95;
        path.Points = new Vector2[] {
            new Vector2(100, 600),
            new Vector2(800, 500),
            new Vector2(1600, 800),
            new Vector2(2400, 400),
            new Vector2(3200, 700),
            new Vector2(3850, 500)
        };
        path.BeginCapMode = Line2D.LineCapMode.Round;
        path.EndCapMode = Line2D.LineCapMode.Round;
        path.JointMode = Line2D.LineJointMode.Round;
        AddChild(path);
    }

    private void BuildBoundaries()
    {
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, -50), new Vector2(MAP_WIDTH, 100)); // Top
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, MAP_HEIGHT + 50), new Vector2(MAP_WIDTH, 100)); // Bottom
        CreateStaticWall(new Vector2(-50, MAP_HEIGHT / 2), new Vector2(100, MAP_HEIGHT)); // Left
    }

    private void CreateStaticWall(Vector2 pos, Vector2 size)
    {
        var wall = new StaticBody2D { Position = pos };
        var shape = new CollisionShape2D();
        var rect = new RectangleShape2D { Size = size };
        shape.Shape = rect;
        wall.AddChild(shape);
        AddChild(wall);
    }

    private void BuildVegetation()
    {
        for (int i = 0; i < 120; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * MAP_WIDTH,
                (float)_rng.NextDouble() * MAP_HEIGHT
            );

            if (_grassTexture != null)
            {
                var grass = new Sprite2D();
                grass.Texture = _grassTexture;
                grass.Position = pos;
                // Grass is scaled ~0.05 to be 1/5 of player (who is ~0.25)
                float baseScale = 0.05f;
                grass.Scale = new Vector2(baseScale + (float)_rng.NextDouble() * 0.02f, baseScale + (float)_rng.NextDouble() * 0.02f);
                grass.SelfModulate = new Color(0.4f, 0.5f, 0.4f, 0.8f); // Muted cave grass
                grass.ZIndex = -90;
                AddChild(grass);
            }
            else
            {
                var poly = new Polygon2D();
                poly.Polygon = new Vector2[] { new Vector2(0,0), new Vector2(5,-10), new Vector2(10,0) };
                poly.Color = new Color(0.2f, 0.4f, 0.2f);
                poly.Position = pos;
                poly.ZIndex = -90;
                AddChild(poly);
            }
        }
    }

    private void BuildObstacles()
    {
        for (int i = 0; i < 70; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * (MAP_WIDTH - 400) + 200,
                (float)_rng.NextDouble() * (MAP_HEIGHT - 200) + 100
            );

            if (pos.DistanceTo(new Vector2(3850, 500)) < 300) continue;
            CreateOccludingRock(pos);
        }
    }

    private void CreateOccludingRock(Vector2 pos)
    {
        var rockContainer = new Node2D { Position = pos, YSortEnabled = true };
        
        // Visual
        var sprite = new Sprite2D();
        sprite.Texture = _rockTexture;
        float s = 1.2f + (float)_rng.NextDouble() * 2.5f;
        sprite.Scale = new Vector2(s, s);
        sprite.SelfModulate = StonyGray.Darkened((float)_rng.NextDouble() * 0.3f);
        sprite.Offset = new Vector2(0, -sprite.Texture.GetSize().Y / 4); 
        rockContainer.AddChild(sprite);

        // Blocker
        var blocker = new StaticBody2D();
        var bShape = new CollisionShape2D();
        var bRect = new RectangleShape2D { Size = new Vector2(60 * s, 30 * s) };
        bShape.Shape = bRect;
        blocker.AddChild(bShape);
        rockContainer.AddChild(blocker);

        // Occlusion Trigger
        var detector = new Area2D();
        detector.CollisionLayer = 0;
        detector.CollisionMask = 1; // Standard Player Layer
        var dShape = new CollisionShape2D();
        var dRect = new RectangleShape2D { Size = new Vector2(70 * s, 80 * s) };
        dShape.Shape = dRect;
        dShape.Position = new Vector2(0, -40 * s);
        detector.AddChild(dShape);
        rockContainer.AddChild(detector);

        detector.BodyEntered += (body) => {
            if (body is Node2D n && (n.IsInGroup("player") || n.Name.ToString().ToLower().Contains("player"))) {
                var t = CreateTween();
                t.TweenProperty(sprite, "modulate:a", 0.4f, 0.3f);
            }
        };
        detector.BodyExited += (body) => {
            if (body is Node2D n && (n.IsInGroup("player") || n.Name.ToString().ToLower().Contains("player"))) {
                var t = CreateTween();
                t.TweenProperty(sprite, "modulate:a", 1.0f, 0.3f);
            }
        };

        AddChild(rockContainer);
    }

    private void BuildHazards()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 pos = new Vector2(
                (float)_rng.NextDouble() * (MAP_WIDTH - 600) + 300,
                (float)_rng.NextDouble() * (MAP_HEIGHT - 200) + 100
            );

            if (_spikeTrapScene == null) continue;
            var trap = _spikeTrapScene.Instantiate<Node2D>();
            trap.Position = pos;
            trap.Modulate = new Color(0.6f, 0.6f, 0.7f, 0.5f); // Partially hidden
            AddChild(trap);
        }
    }

    private void SetupFallingRocks()
    {
        _rockTimer = new Timer();
        _rockTimer.WaitTime = 2.5f;
        _rockTimer.Autostart = true;
        _rockTimer.Timeout += SpawnFallingRock;
        AddChild(_rockTimer);
    }

    private void SpawnFallingRock()
    {
        var players = GetTree().GetNodesInGroup("player");
        Vector2 spawnPos;

        if (players.Count > 0 && players[0] is Node2D p)
        {
            spawnPos = new Vector2(p.Position.X + (float)(_rng.NextDouble() * 400 - 200), -50);
        }
        else
        {
            spawnPos = new Vector2((float)_rng.NextDouble() * MAP_WIDTH, -50);
        }

        var rock = new Sprite2D();
        rock.Texture = _rockTexture;
        rock.Position = spawnPos;
        rock.Scale = new Vector2(0.5f, 0.5f);
        rock.Modulate = new Color(0.8f, 0.7f, 0.7f);
        rock.ZIndex = 50; // Above everything as it falls
        AddChild(rock);

        var tween = CreateTween();
        float targetY = MAP_HEIGHT + 100;
        tween.TweenProperty(rock, "position:y", targetY, 1.2f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
        tween.Finished += () => rock.QueueFree();
    }

    private void BuildExitPortal()
    {
        if (_exitScene == null) return;
        var exit = _exitScene.Instantiate<Node2D>();
        exit.Position = new Vector2(3850, 500);
        AddChild(exit);

        var light = new PointLight2D();
        light.Texture = CreateRadialGradient(300);
        light.TextureScale = 6.0f;
        light.Color = new Color(0.5f, 0.7f, 1.0f);
        light.Energy = 2.0f;
        light.Position = new Vector2(3850, 500);
        AddChild(light);
    }

    private Texture2D CreateRadialGradient(int size)
    {
        var img = Image.Create(size, size, false, Image.Format.Rgba8);
        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = new Vector2(x, y).DistanceTo(center);
                float alpha = Mathf.Clamp(1.0f - (dist / radius), 0, 1);
                img.SetPixel(x, y, new Color(1, 1, 1, alpha * alpha));
            }
        }
        return ImageTexture.CreateFromImage(img);
    }
}
