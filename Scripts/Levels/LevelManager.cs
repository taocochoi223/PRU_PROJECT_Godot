using Godot;
using System.Collections.Generic;

public partial class LevelManager : Node2D
{
    [Export] public int LevelNumber = 1;
    [Export] public PackedScene PlayerScene;

    private Node2D _spawnPoint;
    private HUD _hud;
    private Player _player;

    // Checkpoint system
    private List<Vector2> _checkpoints = new List<Vector2>();
    private int _currentCheckpoint = 0;

    public override void _Ready()
    {
        // Set current level
        GameManager.Instance.CurrentLevel = LevelNumber;

        // ── PHẦN CỦA BẠN: Reset kĩ năng nếu là màn 1 ──
        if (LevelNumber == 1)
        {
            GameManager.Instance.UnlockedSkillsCount = 0;
            GD.Print("Level 1: Reset UnlockedSkillsCount to 0");
        }

        // ── Setup level ───────────────────────────────────────────
        if (HasNode("SpawnPoint"))
            _spawnPoint = GetNode<Node2D>("SpawnPoint");

        CollectCheckpoints();
        SpawnPlayer();
        ConnectPlayerSignals();

        // ── PHẦN TRÊN GIT: Sinh bẫy đá rơi ──
        if (LevelNumber == 1)
        {
            SpawnLevel1CustomTraps();
        }
    }

    private void SpawnLevel1CustomTraps()
    {
        // Sinh 3 đá rơi bất ngờ (rơi từ trên cao màn hình xuống)
        var rock1 = new FallingRockTrap();
        rock1.Position = new Vector2(750, -50); 
        rock1.TriggerRange = 600f;              
        AddChild(rock1);

        var rock2 = new FallingRockTrap();
        rock2.Position = new Vector2(1750, 0); 
        rock2.TriggerRange = 550f;
        AddChild(rock2);

        var rock3 = new FallingRockTrap();
        rock3.Position = new Vector2(3000, -100); 
        rock3.TriggerRange = 650f;
        AddChild(rock3);
    }

    private void CollectCheckpoints()
    {
        _checkpoints.Clear();
        if (_spawnPoint != null)
        {
            _checkpoints.Add(_spawnPoint.GlobalPosition);
        }

        foreach (var child in GetChildren())
        {
            if (child is Marker2D marker && child.Name.ToString().StartsWith("Checkpoint"))
            {
                _checkpoints.Add(marker.GlobalPosition);
            }
        }

        _checkpoints.Sort((a, b) => a.X.CompareTo(b.X));
        GD.Print($"LevelManager: Tìm thấy {_checkpoints.Count} checkpoint(s)");
    }

    private void SpawnPlayer()
    {
        Vector2 spawnPos = _checkpoints.Count > _currentCheckpoint
            ? _checkpoints[_currentCheckpoint]
            : (_spawnPoint?.GlobalPosition ?? Vector2.Zero);

        if (PlayerScene != null)
        {
            _player = PlayerScene.Instantiate<Player>();
            _player.GlobalPosition = spawnPos;
            _player.AddToGroup("player");
            AddChild(_player);
        }
        else if (HasNode("Player"))
        {
            _player = GetNode<Player>("Player");
            _player.GlobalPosition = spawnPos;
            _player.AddToGroup("player");
        }
    }

    private void ConnectPlayerSignals()
    {
        if (_player == null)
        {
            var node = GetTree().GetFirstNodeInGroup("player");
            if (node is Player p) _player = p;
        }

        if (_player != null)
        {
            _player.PlayerDied += OnPlayerDied;
        }
    }

    public void ActivateCheckpoint(int index)
    {
        if (index > _currentCheckpoint && index < _checkpoints.Count)
        {
            _currentCheckpoint = index;
            GD.Print($"Checkpoint {index} activated!");
        }
    }

    private void OnPlayerDied()
    {
        var timer = GetTree().CreateTimer(1.2);
        timer.Timeout += () =>
        {
            if (!IsInstanceValid(this)) return;
            GameManager.Instance.GameOver();
        };
    }

    public override void _Process(double delta)
    {
        if (_player == null || _player.IsQueuedForDeletion()) return;

        for (int i = _currentCheckpoint + 1; i < _checkpoints.Count; i++)
        {
            float distToCheckpoint = Mathf.Abs(_player.GlobalPosition.X - _checkpoints[i].X);
            if (distToCheckpoint < 60f)
            {
                ActivateCheckpoint(i);
                break;
            }
        }
    }
}