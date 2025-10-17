using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class WitchBody : EnemyBody
{
    [Export] private AnimationPlayer anim;
    [Export] private Sprite2D wand;
    [Export] private Node2D wandTip;
    [Export] private Sprite2D broom;

    [Export] private PackedScene boltPrefab;
    [Export] private PackedScene fireballPrefab;

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 128.0f;


    [Export]
    public int attackDamage = 3;

    [Export]
    public float attackPushForce = 32f;

    [Export]
    public float attackCooldown = 0.1f;

    [Export]
    public float fireballCooldown = 4f;


    // inner variables
    public virtual bool StartWithDefaultController { get => true; }
    public override AnyController DefaultController => new MeleeAIController();

    public override void Move(Vector2 direction)
    {
        base.Move(direction);
        broom.LookAt(broom.GlobalPosition + direction);
        
    }
    public override void Aim(Vector2 direction)
    {
        base.Aim(direction);
        wand.LookAt(wand.GlobalPosition + direction);
    }


    public override void Button1(bool pressed)
    {

    }


    public override void Button2(bool pressed)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (stunned) { MoveAndSlide(); return; }

        Vector2 currentVelocity = Velocity;
        currentVelocity = (moveDirection * speed);

        Velocity = currentVelocity;

        MoveAndSlide();
    }


}  