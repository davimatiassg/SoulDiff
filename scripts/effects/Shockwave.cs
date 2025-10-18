using Godot;
using System;

public partial class Shockwave : DamageEffect
{
    public float knockback = 640f;
    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is not Hitable hit) return;

        if (CheckHitability(hit))
        {
            hit.TakeDamage(damage,  (body.Position - Position).Normalized() * knockback);
        }

    }
    
    public override void _Ready()
	{
		base._Ready();
        animation.AnimationFinished += Dispawn;
	}
}
