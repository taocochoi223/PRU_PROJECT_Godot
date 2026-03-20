using Godot;
using System;

public partial class MiniMap : Control
{
    private Camera2D _mainCamera;
    private Node2D _player;
    private ColorRect _playerDot;
    private ColorRect _mapContent;
    
    // Tỉ lệ thu nhỏ từ map thật sang minimap
    // Map thật: 4800x1200, Minimap: 200x50
    private float _scaleX;
    private float _scaleY;

    // Quản lý các chấm đỏ đại diện cho quái
    private System.Collections.Generic.Dictionary<Node2D, ColorRect> _enemyMarkers = new();

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        _mapContent = GetNode<ColorRect>("MiniMapFrame/MapContent");
        _playerDot = GetNode<ColorRect>("MiniMapFrame/MapContent/PlayerMarker");

        // Đảm bảo PlayerMarker luôn nằm trên cùng
        _playerDot.ZIndex = 10;

        // Tính toán tỉ lệ (MAP_W = 4000, MAP_H = 1200)
        _scaleX = _mapContent.Size.X / 4000f;
        _scaleY = _mapContent.Size.Y / 1200f;

        // Vẽ phác thảo đường đi đơn giản lên minimap
        DrawCaveMiniMap();
    }

    private void DrawCaveMiniMap()
    {
        // 1. Vẽ nền hang động (Màu xám-xanh tối)
        var caveBase = new ColorRect();
        caveBase.Color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        caveBase.Size = _mapContent.Size;
        _mapContent.AddChild(caveBase);

        // 2. Vẽ Đường mòn (Vệt sáng xanh nhạt)
        Vector2[] pathPoints = {
            new(100, 500), new(600, 450), new(1200, 700), new(1800, 300), 
            new(2400, 600), new(3000, 400), new(3500, 700), new(3900, 500)
        };
        var pathLine = new Line2D();
        pathLine.DefaultColor = new Color(0.4f, 0.4f, 1.0f, 0.3f);
        pathLine.Width = 4f;
        pathLine.BeginCapMode = Line2D.LineCapMode.Round;
        pathLine.EndCapMode = Line2D.LineCapMode.Round;
        foreach (var p in pathPoints) {
            pathLine.AddPoint(new Vector2(p.X * _scaleX, p.Y * _scaleY));
        }
        _mapContent.AddChild(pathLine);

        // 3. Tinh thể pha lê (Xanh cyan sáng)
        Vector2[] crystals = { 
            new(1100, 650), new(1300, 750), new(1750, 250), new(1850, 350),
            new(2950, 450), new(3050, 350), new(3450, 650), new(3550, 750)
        };
        foreach (var p in crystals) AddMarker(p, new Color(0.2f, 0.8f, 1.0f, 0.6f), new Vector2(4, 4));

        // 4. Bẫy chông/hố sâu (Màu đỏ cảnh báo)
        Vector2[] hazards = { 
            new(1500, 500), new(2100, 450), new(2700, 550), new(3300, 500),
            new(800, 550), new(2300, 300), new(3700, 600)
        };
        foreach (var p in hazards) AddMarker(p, new Color(1.0f, 0.2f, 0.2f, 0.8f), new Vector2(5, 5));

        // 5. CỔNG RA (Điểm đến cuối cùng)
        AddMarker(new Vector2(3850, 500), new Color(1.0f, 0.9f, 0.2f, 1.0f), new Vector2(12, 12));
        var exitLabel = new Label();
        exitLabel.Text = "EXIT";
        exitLabel.HorizontalAlignment = HorizontalAlignment.Center;
        exitLabel.Scale = new Vector2(0.4f, 0.4f);
        exitLabel.Position = new Vector2(3830 * _scaleX, 470 * _scaleY);
        _mapContent.AddChild(exitLabel);
    }

    private void AddMarker(Vector2 worldPos, Color color, Vector2 size)
    {
        var dot = new ColorRect();
        dot.Color = color;
        dot.Size = size;
        dot.Position = new Vector2(worldPos.X * _scaleX, worldPos.Y * _scaleY) - (size / 2);
        _mapContent.AddChild(dot);
    }

    public override void _Process(double delta)
    {
        if (_player != null && IsInstanceValid(_player))
        {
            // Cập nhật vị trí chấm đỏ của người chơi trên minimap
            Vector2 mapPos = new Vector2(
                _player.GlobalPosition.X * _scaleX,
                _player.GlobalPosition.Y * _scaleY
            );
            
            // Giới hạn trong khung minimap
            mapPos.X = Mathf.Clamp(mapPos.X, 0, _mapContent.Size.X);
            mapPos.Y = Mathf.Clamp(mapPos.Y, 0, _mapContent.Size.Y);
            
            _playerDot.Position = mapPos - (_playerDot.Size / 2);
            
            // Đảm bảo chấm đỏ luôn nằm trên cùng của các Marker khác
            _mapContent.MoveChild(_playerDot, _mapContent.GetChildCount() - 1);
        }
        else
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        }

        // LUÔN CẬP NHẬT VỊ TRÍ QUÁI MỖI FRAME
        UpdateEnemyMarkers();
    }

    private void UpdateEnemyMarkers()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        
        // 1. Tạo hoặc cập nhật vị trí quái hiện tại
        foreach (var node in enemies)
        {
            if (node is Node2D enemyNode && IsInstanceValid(enemyNode))
            {
                // Nếu quái đã chết (nếu có thuộc tính IsDead), ta coi như không còn trên map
                bool isDead = (bool)enemyNode.Get("IsDead");
                if (isDead) continue;

                if (!_enemyMarkers.ContainsKey(enemyNode))
                {
                    // Tạo chấm đỏ mới cho quái
                    var dot = new ColorRect();
                    dot.Color = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Đỏ chói rực rỡ
                    dot.Size = new Vector2(5, 5); // Chấm nhỏ hơn chấm người chơi
                    _mapContent.AddChild(dot);
                    _enemyMarkers[enemyNode] = dot;
                }

                // Cập nhật vị trí
                Vector2 mPos = new Vector2(enemyNode.GlobalPosition.X * _scaleX, enemyNode.GlobalPosition.Y * _scaleY);
                _enemyMarkers[enemyNode].Position = mPos - (_enemyMarkers[enemyNode].Size / 2);
                _enemyMarkers[enemyNode].Visible = true;
            }
        }

        // 2. Dọn dẹp quái đã bị tiêu diệt
        var killedEnemies = new System.Collections.Generic.List<Node2D>();
        foreach (var kvp in _enemyMarkers)
        {
            if (!IsInstanceValid(kvp.Key) || (bool)kvp.Key.Get("IsDead"))
            {
                kvp.Value.QueueFree();
                killedEnemies.Add(kvp.Key);
            }
        }
        foreach (var k in killedEnemies) _enemyMarkers.Remove(k);
    }
}
