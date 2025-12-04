using Godot;
using System;
using System.Diagnostics;

public partial class HudManager : Control
{

    public static HudManager Instance;
    [Export] public TextureProgressBar hpBar;
    [Export] public TextureProgressBar AbilityBar1;
    [Export] public TextureProgressBar AbilityBar2;




    public static void TriggerCooldown(int abilityBar, float cd)
    {
        GD.Print($"cd da abilidade {abilityBar} = {cd}");

        TextureProgressBar bar = abilityBar == 1 ? Instance.AbilityBar1 : abilityBar == 2 ? Instance.AbilityBar2 : null;
        Debug.Assert(bar != null);

        bar.Value = cd;
        bar.MaxValue = cd;
        Tween tween = bar.CreateTween();
        tween.TweenProperty(bar, "value", 0, cd);
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
