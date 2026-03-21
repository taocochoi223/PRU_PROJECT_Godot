using Godot;

/// <summary>
/// Camera that follows the player smoothly.
/// Attach to a Camera2D node that is a child of the Level.
/// </summary>
public partial class FollowCamera : Camera2D
{
    [Export] public float SmoothSpeed = 5.0f;
    [Export] public Vector2 FollowOffset = new Vector2(0, -50);
    [Export] public float MinX = 0;
    [Export] public float MaxX = 3000;
    [Export] public float MinY = 0;
    [Export] public float MaxY = 648;
    [Export] public Vector2 ZoomLevel = new Vector2(1, 1);


    private float _shakeIntensity = 0f;
    private float _shakeTimer = 0f;
    private Node2D _target;

    public override void _Ready()
    {
        // Make this the current camera
        MakeCurrent();
        AddToGroup("MainCamera");

        // Khóa hẳn giới hạn hiển thị của Camera để không quay lố ra vùng không có Map (màu xám)
        LimitLeft = (int)MinX;
        LimitRight = (int)MaxX;
        LimitTop = (int)MinY;
        LimitBottom = (int)MaxY;
        
        Zoom = ZoomLevel;
    }


    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (_target == null || _target.IsQueuedForDeletion())
        {
            // Find player
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player is Node2D p)
            {
                _target = p;
            }
        }

        // Smooth follow
        if (_target != null)
        {
            Vector2 targetPos = _target.GlobalPosition + FollowOffset;
            
            // Sử dụng trực tiếp các thuộc tính Limit thay vì MinX/MaxX cố định
            // Điều này cho phép LevelManager thay đổi LimitRight mà Camera vẫn follow được
            targetPos.X = Mathf.Clamp(targetPos.X, LimitLeft, LimitRight);
            targetPos.Y = Mathf.Clamp(targetPos.Y, LimitTop, LimitBottom);
            
            GlobalPosition = GlobalPosition.Lerp(targetPos, SmoothSpeed * dt);
        }


        // Handle Shake
        if (_shakeTimer > 0)
        {
            _shakeTimer -= dt;
            Offset = new Vector2(
                (float)(GD.RandRange(-1.0, 1.0) * _shakeIntensity),
                (float)(GD.RandRange(-1.0, 1.0) * _shakeIntensity)
            );

            if (_shakeTimer <= 0)
            {
                Offset = Vector2.Zero;
            }
        }
    }

    public void Shake(float duration, float intensity)
    {
        _shakeTimer = duration;
        _shakeIntensity = intensity;
    }
}
