using System;
using System.Threading.Tasks;
using Godot;

public partial class GhostBody : ABody
{   

    [ExportGroup("Balance Variables")]
    [Export] private float speed = 400.0f;
    [Export] private float acel = 400.0f;

    [Export] private float flingForce = 5f;
    [Export] private int flingDamage = 3;

    [ExportGroup("Extras & Cosmetics")]
    [Export] private PackedScene ghostPebblePrefab;

    /// Cosmetic Tools
    [Export] public AnimatedSprite2D skull;
    [Export] public AnimatedSprite2D skullGlow;


    [Export] public Line2D ghostTrail;

   
    [Export] private float trailAcel;

    [ExportGroup("")]
     private const int TRAIL_LEN = 20;
    private Vector2[] trailLastPoints = new Vector2[TRAIL_LEN];



    // Inner Variables
    private Vector2 moveDirection = Vector2.Zero;

    private Vector2 aimDirection = Vector2.Zero;

    
    public override void PossessStart(PlayerController cntrl)
    {
        base.PossessStart(cntrl);
        HP = 2;

    }

    public override void Button1()
    {
        GhostPebble pebble = (GhostPebble)EffectPool.SpawnEffect(ghostPebblePrefab, GetParent());
        pebble.Fling(aimDirection * flingForce, flingDamage);
        pebble.Position = Position;
    }

    public override void Button2()
    {
        //dash
    }

    public override void Button3()
    {
        //posssess
    }

    public override void Move(Vector2 direction)
    {
        moveDirection = direction;
    }
    public override void Aim(Vector2 direction)
    {
        aimDirection = direction;
    }

    public override void TakeDamage(int damage, Vector2 knockback)
    {
        base.TakeDamage(damage, knockback);
        if (HP > 0)
        {
            Action a = async () =>
            {
                if (hasDamageFrames) vulnerable = false;
                skull.Play("damaged");
                skullGlow.Play("damaged");
                await Task.Delay(invincibilityTime);
                vulnerable = false;
                skull.Play("idle");
                skullGlow.Play("idle");
            };
            a();
        }


    }

    public override void PossessEnd()
    {
        base.PossessEnd();
        if (HP <= 0) Die();
    }

    //TODO!
    public override void Die()
    {
        base.Die();
    }





    public override void _Ready()
    {
        base._Ready();
        for (int i = 0; i < TRAIL_LEN; i++)
        {
            trailLastPoints[i] = Vector2.Zero;
        }

        Tween tweenGlowColor = CreateTween();
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(1, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 1, 1), .5);
        tweenGlowColor.TweenProperty(skullGlow, "modulate", new Color(0, 0, 1), 1);
        tweenGlowColor.SetLoops();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 currentVelocity = Velocity;

        

        if (moveDirection != Vector2.Zero)
        {
            currentVelocity = currentVelocity.MoveToward(moveDirection * speed, (float)delta * acel *
                (currentVelocity.LengthSquared()/(moveDirection + currentVelocity).LengthSquared() + 1f)
            );
        }
        else
        {
            currentVelocity = currentVelocity.MoveToward(Vector2.Zero, (float)delta * acel * 1.5f);
        }

        Velocity = currentVelocity;

        MoveAndSlide();
    }

    

    private void CalculateTrail(float delta)
    {
        ghostTrail.Position = -Position;

        for (int i = 0; i < ghostTrail.Points.Length; i++)
        {
            Vector2 p = ghostTrail.GetPointPosition(i);
            ghostTrail.SetPointPosition(i, trailLastPoints[i] + Vector2.Down * delta * trailAcel );
            trailLastPoints[i] = p;
        }

        ghostTrail.AddPoint(Position);

        while (ghostTrail.Points.Length > TRAIL_LEN) ghostTrail.RemovePoint(0);  
    }

    

    
    public override void _Process(double delta)
    {
        base._Process(delta);
        CalculateTrail((float)delta);

        // Vector2 p = GlobalPosition;
        // if (moveDirection == Vector2.Zero) p += Vector2.Down;
        // else ghostTrail.Position = -p;
        // if (p != ghostTrail.Points.LastOrDefault()) ghostTrail.AddPoint(p);



    }
}
