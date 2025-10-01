using Godot;
using System;
using System.Collections.Generic;

public partial class GhostPebble : Effect
{
    Vector2 velocity = Vector2.Zero;
    int damage = 0;
    public override void _Ready()
    {
        base._Ready();
    }
    public void Fling(Vector2 velocity, int damage)
    {
        this.damage = damage;
        this.velocity = velocity;
        SetExitTime(5000);
    }

    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is Hitable hit)
        {
            if (hit is ABody creature) { if (creature.isPossessed) return; }

            hit.TakeDamage(damage, velocity * 0.1f);
        }

        Dispawn();
    }

    public override void _Process(double delta)
    {
        base._PhysicsProcess(delta);
        Translate((float)delta * velocity);
        
    }
    
}