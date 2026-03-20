using Godot;
using System;

/// <summary>
/// Script giúp cây trở nên trong suốt khi nhân vật đi vào vùng tán cây
/// </summary>
public partial class TreeTransparency : Sprite2D
{
    [Export] public float TargetAlpha = 0.3f;
    [Export] public float FadeDuration = 0.25f;
    [Export] public float DetectionRadius = 60.0f;
    [Export] public Vector2 DetectionOffset = new Vector2(0, -50);

    private float _originalAlpha = 1.0f;

    public override void _Ready()
    {
        _originalAlpha = Modulate.A;

        // Tạo Area2D để nhận diện Player nếu chưa có
        Area2D area = GetNodeOrNull<Area2D>("DetectionArea");
        if (area == null)
        {
            area = new Area2D();
            area.Name = "DetectionArea";
            area.CollisionLayer = 0;
            area.CollisionMask = 1; // Player thường ở Layer 1
            AddChild(area);

            CollisionShape2D col = new CollisionShape2D();
            CircleShape2D shape = new CircleShape2D();
            shape.Radius = DetectionRadius;
            col.Shape = shape;
            col.Position = DetectionOffset;
            area.AddChild(col);
        }

        // Kết nối sự kiện
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            FadeTo(TargetAlpha);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            FadeTo(_originalAlpha);
        }
    }

    private void FadeTo(float alpha)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", alpha, FadeDuration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
    }

    /// <summary>
    /// Hàm tiện ích để áp dụng script này và cấu hình cho một Sprite2D bất kỳ
    /// </summary>
    public static void ApplyTo(Sprite2D sprite, float radius = 60f, Vector2? offset = null)
    {
        if (sprite == null) return;
        
        // Thêm script nếu chưa có (lưu ý: Trong Godot C#, việc gán script lúc runtime phức tạp hơn GDScript)
        // Thay vào đó, ta có thể thêm logic trực tiếp vào node nếu gọi từ một script khác.
    }
}
