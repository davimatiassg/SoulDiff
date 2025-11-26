using Godot;
using System;

public interface Hitable
{
    void TakeDamage(int damage, Vector2 knockback);
}