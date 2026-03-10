using Godot;
using System;

public partial class PopupInputHelper : Node
{
    private int _slideCount = 0;
    private int _maxSlides = 2;
    private Node _overlay;

    public override void _Input(InputEvent @event)
    {
        if (@event.IsPressed() && (@event is InputEventKey || @event is InputEventMouseButton))
        {
            _slideCount++;
            if (_slideCount >= _maxSlides)
            {
                QueueFree();
            }
            else
            {
                // Tìm đến label trong overlay để đổi text
                // Cách này hơi "hacky" nhưng hiệu quả cho popup nhanh
                GetParent().Call("OnNextSlide");
            }
            GetViewport().SetInputAsHandled();
        }
    }
}
