using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public abstract partial class EnemyBody : AnyBody
{

    public override void Button3(bool pressed)
    {
        //explode and deal damage & stuff
        GameManager.PossessionDown();
    }
}
