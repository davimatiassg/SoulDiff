using Godot;
using System;

public partial class LevelManager : Node2D
{
    public override void _Ready()
    {
        base._Ready();

        AudioPlayer.Play("Level 1", true, 1f, 0.2f);
    }
}
