using Godot;
using System;

public partial class DamageEffect : Effect
{
    public int damage = 3;
    public bool playerEffect = false;

    public bool CheckHitability(Hitable hit)
    {
        if(hit is not AnyBody body) return true;
        
        return body.isPlayer != playerEffect;
    }

}
