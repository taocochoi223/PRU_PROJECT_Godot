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

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        _mapContent = GetNode<ColorRect>("MiniMapFrame/MapContent");
        _playerDot = GetNode<ColorRect>("MiniMapFrame/MapContent/PlayerMarker");

        // Đảm bảo PlayerMarker luôn nằm trên cùng
        _playerDot.ZIndex = 10;

        // Tính toán tỉ lệ (MAP_W = 4800, MAP_H = 1200)
        _scaleX = _mapContent.Size.X / 4800f;
        _scaleY = _mapContent.Size.Y / 1200f;

        // Vẽ phác thảo đường đi đơn giản lên minimap
        DrawSimplePath();
    }

    private void DrawSimplePath()
    {
        // 1. Vẽ nền rừng (Màu xanh rừng tối, bo góc)
        var forestBase = new ColorRect();
        forestBase.Color = new Color(0.1f, 0.2f, 0.1f, 0.85f);
        forestBase.Size = _mapContent.Size;
        _mapContent.AddChild(forestBase);

        // 2. Vẽ Đường mòn (Vệt sáng nghệ thuật)
        Vector2[] pathPoints = {
            new(100, 600), new(400, 550), new(700, 500), new(1000, 550),
            new(1300, 600), new(1600, 500), new(1900, 450), new(2200, 500),
            new(2500, 550), new(2800, 500), new(3100, 450), new(3400, 500),
            new(3700, 550), new(4000, 500), new(4300, 500), new(4600, 550)
        };
        var pathLine = new Line2D();
        pathLine.DefaultColor = new Color(0.8f, 0.7f, 0.4f, 0.4f);
        pathLine.Width = 3f;
        pathLine.BeginCapMode = Line2D.LineCapMode.Round;
        pathLine.EndCapMode = Line2D.LineCapMode.Round;
        foreach (var p in pathPoints) {
            pathLine.AddPoint(new Vector2(p.X * _scaleX, p.Y * _scaleY));
        }
        _mapContent.AddChild(pathLine);

        // 3. Vẽ Vũng nước (Xanh lơ)
        Vector2[] ponds = { new(1400, 350), new(2600, 750), new(800, 200), new(3200, 900) };
        foreach (var p in ponds) AddMarker(p, new Color(0.3f, 0.6f, 1.0f, 0.7f), new Vector2(12, 6));

        // 4. Vẽ Hố sâu (Đen huyền bí)
        Vector2[] pits = { new(1200, 600), new(2500, 850), new(3800, 400), new(1800, 300), new(1800, 500) };
        foreach (var p in pits) AddMarker(p, new Color(0, 0, 0, 0.9f), new Vector2(8, 4));

        // 5. Đá & Vật cản (Xám bạc)
        Vector2[] rocks = { new(350, 300), new(900, 750), new(1600, 200), new(2100, 850), new(2900, 300), new(3600, 800) };
        foreach (var p in rocks) AddMarker(p, new Color(0.7f, 0.7f, 0.8f, 0.8f), new Vector2(4, 4));

        // 6. CỔNG HANG (Điểm đến cuối cùng - Biểu tượng đặc biệt)
        AddMarker(new Vector2(4500, 500), new Color(1.0f, 0.8f, 0.2f, 1.0f), new Vector2(10, 10)); // Ô vuông màu vàng sáng
        var caveLabel = new Label();
        caveLabel.Text = "EXIT";
        caveLabel.HorizontalAlignment = HorizontalAlignment.Center;
        caveLabel.Scale = new Vector2(0.35f, 0.35f);
        caveLabel.Position = new Vector2(4480 * _scaleX, 480 * _scaleY);
        _mapContent.AddChild(caveLabel);
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
    }
}
