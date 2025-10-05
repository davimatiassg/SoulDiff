using Godot;
using System;

public interface Hitable
{
    public const string fx = "res://Scenes/HitFX.tscn";
    void TakeDamage(int damage, Vector2 knockback);
}