using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{
    public const float DEAD_TIME = 5f;
    [Export] public AnimationPlayer anim;
    public virtual bool StartWithDefaultController { get => true; }
    public virtual AnyController DefaultController => new MeleeAIController();
    public override void Button3(bool pressed)
    {
        if (!pressed) return;
        Die();
        QueueFree();
    }

    public override void _Ready()
    {
        base._Ready();
        if (StartWithDefaultController) DefaultController.Connect(this);

        
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);
        if (HP <= 0)
        {
            if (isPlayer) PlayerController.Disembody(this); 
            else OutlineColor = Colors.Red;
        }
    }

    public override void Die()
    {
        dead = true;


        if (isPlayer)
        {
            PlayerController.Disembody(this);
            QueueFree();
            return;
        }

        anim.Play("dead");

        var deadControl = controller;

        controller.Disconnect(this);

        deadControl.QueueFree();
        
        Tween tween = CreateTween();
        tween.TweenInterval(DEAD_TIME);
        tween.TweenCallback(Callable.From(() =>
        {
            if (controller == null) QueueFree();
        }));
    }
}
