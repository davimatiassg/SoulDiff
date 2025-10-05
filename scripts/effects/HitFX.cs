using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class HitFX : Effect
{
    public override async void _Ready()
    {
        base._Ready();
        await Task.Delay(333);
        this.Dispawn();
    }
}