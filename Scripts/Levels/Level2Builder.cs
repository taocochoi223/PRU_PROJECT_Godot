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
    private static readonly Color DarkCaveColor = new Color("#606068"); // Lighter ambient for "standard" cave feel
    private static readonly Color StonyGray = new Color(0.35f, 0.35f, 0.38f);
    private static readonly Color PathColor = new Color("#5a4a3a"); // Darker trail for contrast against light floor

    // Resources
    private Shader _caveStoneShader = GD.Load<Shader>("res://Assets/Shaders/cave_stone.gdshader");
    private Texture2D _rockTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/rock_pixel.png");
    private Texture2D _grassTexture = GD.Load<Texture2D>("res://Assets/Sprites/Environment/grass.png");
    private Texture2D _floorTexture = GD.Load<Texture2D>("res://Assets/Sprites/Backgrounds/cave_ground.png");
    private PackedScene _exitScene = GD.Load<PackedScene>("res://Scenes/Objects/LevelExit.tscn");
    private PackedScene _spikeTrapScene = GD.Load<PackedScene>("res://Scenes/Hazards/SpikeHazard.tscn");

    private Random _rng = new Random();
    private Texture2D _lightTexture;
    private List<int> _rollingLanes = new List<int>();
    private float[] _laneY = { 200f, 500f, 850f }; // North, Middle, South center Y

    public override void _Ready()
    {
        _rng = new Random();
        _lightTexture = CreateRadialGradient(256); // Generate light in-code to avoid import issues

        // 1. Setup Sorting & Layers
        YSortEnabled = true;
        ZIndex = 0;

        // 2. Build Environment
        BuildAtmosphere();
        BuildFloorAndPath();
        BuildBoundaries();
        BuildObstacles();
        BuildHazards();
        BuildExitPortal();
        
        // Ensure PlayerLight in the scene also uses the code-generated texture
        var playerLight = GetParent().GetNodeOrNull<PointLight2D>("PlayerLight");
        if (playerLight != null) {
            playerLight.Texture = _lightTexture;
            playerLight.ShadowEnabled = false; // Disable shadows per user's "always bright" request
            playerLight.Energy = 1.3f;
            playerLight.TextureScale = 4.0f;
        }

        // 3. Setup Dynamic Systems
        BuildRollingRockTraps();
    }

    private void BuildAtmosphere()
    {
        // ColorRect was removed because it clashed with CanvasModulate
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
        floor.SelfModulate = new Color(0.8f, 0.8f, 0.9f); // Lighter ground for better visibility
        AddChild(floor);

        // Define 3 Main Branches
        Vector2[][] branches = new Vector2[][] {
            // Branch 1: North
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 200), new Vector2(1500, 150), new Vector2(3000, 200), new Vector2(3850, 500) },
            // Branch 2: Middle
            new Vector2[] { new Vector2(100, 500), new Vector2(1000, 500), new Vector2(2500, 500), new Vector2(3850, 500) },
            // Branch 3: South
            new Vector2[] { new Vector2(100, 500), new Vector2(400, 800), new Vector2(1500, 950), new Vector2(3000, 800), new Vector2(3850, 500) }
        };

        foreach (var points in branches)
        {
            var path = new Line2D();
            path.Points = points;
            path.DefaultColor = PathColor;
            path.Width = 140f;
            path.ZIndex = -95;
            path.BeginCapMode = Line2D.LineCapMode.Round;
            path.EndCapMode = Line2D.LineCapMode.Round;
            path.JointMode = Line2D.LineJointMode.Round;
            AddChild(path);

            // Path lighting was removed to focus on Player-Centric light only
        }
    }

    public void CreateLightAt(Vector2 position)
    {
        // Path lighting is now disabled for a darker "Fog of War" atmosphere
    }

    private void BuildBoundaries()
    {
        // Simple Top/Bottom invisible walls (Layer 2 for CharacterBody2D)
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, -20), new Vector2(MAP_WIDTH, 100)); // Top
        CreateStaticWall(new Vector2(MAP_WIDTH / 2, 1040), new Vector2(MAP_WIDTH, 100)); // Bottom (Moved up from 1220)
        
        // Horizontal Start/End (Layer 2)
        CreateStaticWall(new Vector2(-50, MAP_HEIGHT / 2), new Vector2(100, MAP_HEIGHT)); // Left
    }

    private void CreateStaticWall(Vector2 pos, Vector2 size)
    {
        var wall = new StaticBody2D { Position = pos };
        wall.CollisionLayer = 2; // Matches Player's Mask 6
        var shape = new CollisionShape2D();
        var rect = new RectangleShape2D { Size = size };
        shape.Shape = rect;
        wall.AddChild(shape);
        AddChild(wall);
    }

    private void BuildObstacles()
    {
        // Solid rock walls (Y=320, Y=680) to split the paths
        // We use Triple Rows to ensure a thick, impassable barrier
        for (int i = 0; i < 150; i++) 
        {
            float x = i * 28 + (float)_rng.NextDouble() * 10;
            // Clear spawn area (first 450px) to ensure visibility
            if (x < 450 || x > MAP_WIDTH - 150) continue; 
            
            // Upper wall Divider (y=310 to 330)
            CreateOccludingWall(new Vector2(x, 310 + (float)_rng.NextDouble() * 20));
            // Lower wall Divider (y=670 to 690)
            CreateOccludingWall(new Vector2(x, 670 + (float)_rng.NextDouble() * 20));

            // Bottom Boundary Wall (Y=1000) to match the other lanes width
            CreateOccludingWall(new Vector2(x, 1000 + (float)_rng.NextDouble() * 20));
        }

        // Boundary walls (edge of map)
        for (int i = 0; i < 80; i++)
        {
            CreateOccludingWall(new Vector2(i * 50, -20));
            // The bottom wall is already handled in BuildObstacles() at Y=1000
        }
    }

    private void CreateOccludingWall(Vector2 pos)
    {
        var wallContainer = new Node2D { Position = pos, YSortEnabled = true };
        var sprite = new Sprite2D();
        sprite.Texture = _rockTexture;
        // Scaled down significantly (0.3 to 0.6) to match player size
        float s = 0.3f + (float)_rng.NextDouble() * 0.3f; 
        sprite.Scale = new Vector2(s, s);
        sprite.SelfModulate = new Color(0.7f, 0.7f, 0.75f); // High visibility light grey
        sprite.Offset = new Vector2(0, -sprite.Texture.GetSize().Y / 4); 
        wallContainer.AddChild(sprite);

        // Solid Body with Collision Shape
        var blocker = new StaticBody2D();
        blocker.CollisionLayer = 2; // Matches Player's Mask 6 (Binary 110)
        blocker.CollisionMask = 0; 

        var bShape = new CollisionShape2D();
        // Smaller, more precise collision for the small rocks
        var bRect = new RectangleShape2D { Size = new Vector2(40 * s, 30 * s) }; 
        bShape.Shape = bRect;
        blocker.AddChild(bShape);
        wallContainer.AddChild(blocker);
        
        AddChild(wallContainer);
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

    private void BuildRollingRockTraps()
    {
        // Pick 2 out of 3 lanes to have rolling rocks
        var allLanes = new List<int> { 0, 1, 2 };
        for (int i = 0; i < 2; i++)
        {
            int idx = _rng.Next(allLanes.Count);
            _rollingLanes.Add(allLanes[idx]);
            allLanes.RemoveAt(idx);
        }

        // Place triggers along the paths (Layer 2 for CharacterBody2D)
        foreach (int laneIdx in _rollingLanes)
        {
            float laneY = _laneY[laneIdx];
            for (int x = 600; x < MAP_WIDTH - 600; x += 800)
            {
                CreateRollingRockTrigger(new Vector2(x, laneY), laneY);
            }
        }
    }

    private void CreateRollingRockTrigger(Vector2 pos, float laneY)
    {
        var trigger = new Area2D { Position = pos };
        trigger.CollisionLayer = 0;
        trigger.CollisionMask = 1; // Player

        var shape = new CollisionShape2D();
        shape.Shape = new RectangleShape2D { Size = new Vector2(100, 150) };
        trigger.AddChild(shape);
        AddChild(trigger);

        trigger.BodyEntered += (body) => {
            if (body.IsInGroup("player"))
            {
                // Spawn rock slightly ahead of the trigger
                SpawnRollingRockAt(new Vector2(pos.X + 700, laneY), pos.X - 500);
            }
        };

        // Note: Repeatable as BodyEntered fires each time the player enters.
    }

    private void SpawnRollingRockAt(Vector2 spawnPos, float targetX)
    {
        var rock = new Node2D { Position = spawnPos };
        AddChild(rock);

        var sprite = new Sprite2D();
        sprite.Texture = _rockTexture;
        sprite.Scale = new Vector2(0.6f, 0.6f);
        sprite.SelfModulate = new Color(0.9f, 0.7f, 0.7f); // Slightly distinct
        rock.AddChild(sprite);

        // Movement & Rolling Animation
        var tween = CreateTween().SetParallel(true);
        float duration = 3.0f;

        tween.TweenProperty(rock, "position:x", targetX, duration);
        tween.TweenProperty(sprite, "rotation_degrees", -720, duration);

        // High Damage Detection (75 damage = 50% of 150 HP)
        var area = new Area2D();
        area.CollisionLayer = 0;
        area.CollisionMask = 1; // Player
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 30f };
        area.AddChild(shape);
        rock.AddChild(area);

        area.BodyEntered += (body) => {
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", 75); // 50% HP damage
                rock.QueueFree();
            }
        };

        tween.Chain().Finished += () => rock.QueueFree();
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

    public override void _Process(double delta)
    {
        // Make PlayerLight follow the player
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0 && players[0] is Node2D player)
        {
            var pLight = GetParent().GetNodeOrNull<PointLight2D>("PlayerLight");
            if (pLight != null)
            {
                pLight.GlobalPosition = player.GlobalPosition;
            }
        }
    }

    private Texture2D CreateRadialGradient(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
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
