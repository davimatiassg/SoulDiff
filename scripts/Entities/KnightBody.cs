using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class KnightBody : EnemyBody
{
    [Export] private AnimationPlayer anim;

    [ExportGroup("Balance Variables")]

    [Export]
    public float speed = 128.0f;


    [Export]
    public int swingDamage = 3;

    [Export]
    public float swingCooldown = 1f;
    private float swingCD;

    [Export]
    public float shieldCooldown = 4f;
    public float shieldDuration = 0.5f;

    private bool shielding = false;
    private float defCD;

    // inner variables

    public Vector2 moveDirection = Vector2.Zero;
    public Vector2 aimDirection = Vector2.Zero;

    public override void Move(Vector2 direction)
    {
        moveDirection = direction;
    }
    public override void Aim(Vector2 direction)
    {
        aimDirection = direction;
        sprite.Scale = new Vector2(aimDirection.X < 0 ? -1f : 1f, 1f);
    }

    public override void Button1(bool pressed)
    {
        anim.Play("attack");
    }

    
    public override void Button2(bool pressed)
    {
        if (pressed) Shield();         
        else Unshield();
    }


    private float shieldSpeed = 0f;
    Tween shieldTween;
    private void Shield()
    {
        if (shielding) return;

        var temp = shieldSpeed;
        shieldSpeed = speed;
        speed = temp;

        shieldTween = CreateTween();
        shieldTween.TweenInterval(shieldCooldown);
        shieldTween.TweenCallback(Callable.From(Unshield));

        anim.Play("def");
        shielding = true;
    }
    private void Unshield()
    {
        if (!shielding) return;

        var temp = shieldSpeed;
        shieldSpeed = speed;
        speed = temp;

        shieldTween.Kill();
        anim.Play("idle");
        shielding = false;


    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        if (shielding)
        {
            Unshield();
            //emit damage pulse
            return;
        }
        base.TakeDamage(damage, knockback);

    }

    public void Attack()
    {

    }

    public override void HitstunApply()
    {
        base.HitstunApply();
        anim.Play("hurt");
    }

    public override void HitstunCleanse()
    {
        base.HitstunCleanse();
        anim.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (stunned) { MoveAndSlide();  return; }

        Vector2 currentVelocity = Velocity;

        currentVelocity = (moveDirection * speed);

        anim.Play(currentVelocity.LengthSquared() > 0 ? "walk" : "idle");

        Velocity = currentVelocity;

        MoveAndSlide();
    }

}  