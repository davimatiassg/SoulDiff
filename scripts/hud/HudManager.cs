using Godot;
using System;

public partial class HudManager : Control
{

    public static HudManager Instance;
    [Export] public TextureProgressBar hpBar;
    [Export] public TextureProgressBar AbilityBar1;
    [Export] public TextureProgressBar AbilityBar2;




    public static void TriggerCooldown(int abilityBar, float cd)
    {
        GD.Print($"cd da abilidade {abilityBar} = {cd}");
    }

    public static void ResetHPBar(int maxHP)
    {
        Instance.hpBar.MaxValue = maxHP;
        Instance.hpBar.Value = maxHP;
    }
    
    public static void UpdateHPBar(int hp)
    {
        Instance.hpBar.Value = hp;
    }

    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this) { Instance.QueueFree(); Instance = this; }
    }
}
