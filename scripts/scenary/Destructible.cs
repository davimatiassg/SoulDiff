using Godot;
using System;

public partial class Destructible : Node2D, Hitable
{
    [Export] Node2D sprite;
    [Export] CollisionShape2D collision;
    [Export] GpuParticles2D particles;
    public int HP = 1;

    public void TakeDamage(int damage, Vector2 knockback)
    {
        HP -= damage;
        if (HP <= 0) Destroy();
    }

    public void DisableCollision(){collision.Disabled = true;}
    public void Destroy()
    {
        sprite.Visible = false;
        particles.Emitting = true;
        CallDeferred(MethodName.DisableCollision);
        Tween tween = CreateTween();
        tween.TweenInterval(2);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
