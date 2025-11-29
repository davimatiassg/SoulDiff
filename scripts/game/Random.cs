using Godot;
using System;

public partial class Random : Node
{
    private static RandomNumberGenerator _rng = new RandomNumberGenerator();

    public static float GenerateFloat() => _rng.Randf();

    public static float GenerateFloat(float min, float max) => _rng.RandfRange(min, max);


    public override void _Ready()
    {
        base._Ready();
        _rng.Randomize();
    }
}
