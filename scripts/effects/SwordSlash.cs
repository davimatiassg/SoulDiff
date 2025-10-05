using Godot;
using System;

public partial class SwordSlash : DamageEffect
{
    public int damage = 3;
    public Vector2 direction;
    public float knockback = 16f;
    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is not Hitable hit) return;

        if (CheckHitability(hit))
        {
            hit.TakeDamage(damage, direction * knockback);
        }

    }
    
    public override void _Ready()
	{
		base._Ready();
        animation.AnimationFinished += Dispawn;
	}

    
}
