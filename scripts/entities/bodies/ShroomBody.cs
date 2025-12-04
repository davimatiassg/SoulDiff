using Godot;
using System;

public partial class ShroomBody : EnemyBody
{
    [ExportGroup("Connections")]

    [Export] Sprite2D Glove_0;
    [Export] Sprite2D Glove_1;

    [Export] PackedScene PunchPrefab;
    [Export] PackedScene BombShroomPrefab;




    [ExportGroup("Balance Variables")]

    [Export] public float baseMoveSpeed = 128f;
    [Export] public int attackDamage = 3;
    [Export] public float attackPushForce = 32f;
    [Export] public float attackCooldown = 0.4f;

    // [Export] public float shieldCooldown = 4f;
    // [Export] public float shieldMoveSpeed = 0f;
    private bool attacking;
    private bool moving;

    public override void Move(Vector2 direction)
    {
        if (dead || stunned) return;
        base.Move(direction);

        bool movingToggled = direction.LengthSquared() != 0 != moving;
        if (movingToggled) moving = !moving;

        if (attacking || stunned) return;


        if (movingToggled)
        {
            anim.Play("RESET");
            anim.Play(moving ? "walk" : "idle");
        }

    }
    public override void Aim(Vector2 direction)
    {
        if (dead || stunned) return;
        base.Aim(direction);
    }

    Tween attackTween;
    private bool canAttack = true;
    private bool lastHandRight = false;
    public override void Button1(bool pressed)
    {
        if (dead || !canAttack || attacking || stunned) return;

        anim.Play("RESET");
        anim.Play("attack");
        attacking = true;

        canAttack = false;

        if (isPlayer) HudManager.TriggerCooldown(1, attackCooldown);

        attackTween = CreateTween();
        attackTween.TweenInterval(attackCooldown);
        attackTween.TweenCallback(Callable.From(() => { canAttack = true; }));

        AnimationMixer.AnimationFinishedEventHandler stopAtkAction = (StringName animName) => { };

        stopAtkAction = (StringName animName) =>
        {
            if (animName != "attack") { return; }
            anim.AnimationFinished -= stopAtkAction;
            attacking = false;
            moving = false;
            anim.Play("RESET");
            speed = baseMoveSpeed;

        };

        anim.AnimationFinished += stopAtkAction;
    }

    public override void Button2(bool pressed)
    {
        if (stunned || !pressed || !canFireball ) return;
    }
}
