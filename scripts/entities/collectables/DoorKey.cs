using Godot;
using System;

public partial class DoorKey : Node2D
{
    [Export] Area2D collider;
    [Export] AnimatedSprite2D Sprite;

    public static event Action OnCollectKey;

    public override void _Ready()
    {
        base._Ready();
        OnCollectKey = null;
        collider.BodyShapeEntered += OnCollisionEnter;
    }

    public void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        if (collider == null) return;
        
        if (!(body is AnyBody entityBody && entityBody.isPlayer)) return;

        OnCollectKey?.Invoke();
        Vanish();
    }

    public void Vanish()
    {
        //STUB:
        QueueFree();
    }


}
