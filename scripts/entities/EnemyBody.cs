using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{

    public override void Button3(bool pressed)
    {
        if (pressed) return;
        // 
        GameManager.PossessionDown(this);
        Die();
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);
        if (HP <= 0 && isPlayer) { GameManager.PossessionDown(this); return; }
    }
}
