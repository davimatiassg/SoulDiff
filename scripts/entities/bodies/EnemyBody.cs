using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{
    public const float DEAD_TIME = 5f;
    public const double VANISH_TIME = 2f;
    [Export] public AnimationPlayer anim;
    [Export] public GpuParticles2D deathParticles;

    [Export]
    public virtual bool StartWithDefaultController { get; set; }
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
            else
            {
                OutlineColor = Colors.Red;
                Tween tween = CreateTween();
                tween.TweenProperty(this, "OutlineColor", Colors.Black, DEAD_TIME).SetTrans(Tween.TransitionType.Sine);
            }
            
        }
    }

    public override void PossessStart(PlayerController cntrl)
    {
        base.PossessStart(cntrl);

        CollisionLayer = 1 << 1;
    }

    public override void Die()
    {
        dead = true;

        CollisionLayer = 1 << 0;

        if (isPlayer)
        {
            PlayerController.Disembody(this);
            Vanish();
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
            if (controller == null) Vanish();
        }));
    }

    public void Vanish()
    {
        CollisionLayer = 0;
        sprite.Visible = false;
        deathParticles.Emitting = true;

        Tween deathtween = CreateTween();
        deathtween.TweenInterval(VANISH_TIME);
        deathtween.TweenCallback(Callable.From(QueueFree));
    }
}
