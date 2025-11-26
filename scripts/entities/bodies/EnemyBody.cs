using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{
    [Export] public AnimationPlayer anim;
    public virtual bool StartWithDefaultController { get => true; }
    public virtual AnyController DefaultController => new MeleeAIController();
    public override void Button3(bool pressed)
    {
        if (!pressed) return;
        Die();
    }

    public override void _Ready()
    {
        base._Ready();
        if (StartWithDefaultController) DefaultController.Connect(this);

        HP = MaxHP;
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);
        if (HP <= 0 && isPlayer) { PlayerController.Disembody(this); return; }

        if (HP < MaxHP * (0.2))
        {
            OutlineColor = Colors.Red;

        }

        if (HP <= 0)
        {
            Die();
            return;
        }

    }

    public override void Die()
    {
        base.Die();
    }
}
