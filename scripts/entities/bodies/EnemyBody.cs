using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{

    public abstract AnyController DefaultController { get; }

    public virtual bool StartWithDefaultController { get => true; }

    public override void Button3(bool pressed)
    {
        if (!pressed) return;
        
        GameManager.PossessionDown(this);
        Die();
    }

    public override void _Ready()
    {
        base._Ready();
        if (StartWithDefaultController) GameManager.ConnectBodies(this, DefaultController);

        HP = MaxHP;
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);
        if (HP <= 0 && isPlayer) { GameManager.PossessionDown(this); return; }

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
}
