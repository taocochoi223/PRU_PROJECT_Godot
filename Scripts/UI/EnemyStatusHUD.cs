using Godot;
using System;

public partial class EnemyStatusHUD : CanvasLayer
{
    private Label _countLabel;
    private ColorRect _bg;

    public override void _Ready()
    {
        Layer = 10; // Đảm bảo nằm trên các layer background khác

        // Tạo Background mờ ảo sang trọng
        _bg = new ColorRect();
        _bg.Color = new Color(0, 0, 0, 0.6f);
        _bg.CustomMinimumSize = new Vector2(220, 45);
        _bg.Position = new Vector2(20, 20); // Góc trên bên trái
        
        // Bo góc giả lập bằng CornerRadius nếu là StyleBox, nhưng ở đây dùng đơn giản
        AddChild(_bg);

        // Tạo Label hiển thị số lượng
        _countLabel = new Label();
        _countLabel.Name = "EnemyCountLabel";
        _countLabel.Position = new Vector2(35, 27); // Căn chỉnh trong BG
        _countLabel.AddThemeFontSizeOverride("font_size", 22);
        _countLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.4f)); // Màu vàng ánh kim
        
        // Thêm icon giả lập bằng ký tự đặc biệt
        _countLabel.Text = $"☠ Quái vật: 0 / 0";
        AddChild(_countLabel);

        // Kết nối tín hiệu từ GameManager
        GameManager.Instance.EnemyCountChanged += UpdateCount;
        
        // Cập nhật lần đầu
        UpdateCount(GameManager.Instance.DefeatedEnemies, GameManager.Instance.TotalEnemies);
    }

    private void UpdateCount(int defeated, int total)
    {
        _countLabel.Text = $"☠ Quái vật: {defeated} / {total}";
        
        // Hiệu ứng nháy nhẹ khi có quái chết
        var tw = CreateTween();
        tw.TweenProperty(_countLabel, "modulate", new Color(1, 0.2f, 0.2f), 0.1f);
        tw.TweenProperty(_countLabel, "modulate", Colors.White, 0.2f);
    }
}
