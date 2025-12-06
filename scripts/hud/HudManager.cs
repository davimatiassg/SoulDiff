using Godot;
using System;
using System.Diagnostics;

public partial class HudManager : Control
{

    public static HudManager Instance;
    [Export] public TextureProgressBar hpBar;
    [Export] public TextureRect bodyPortrait;
    [Export] public TextureProgressBar abilityBar1;
    [Export] public TextureProgressBar abilityBar2;

    [Export] public TextureProgressBar autodamageCountdown;



    public static void SetBodyPortrait(AnyBody body)
    {
        Instance.bodyPortrait.Texture = body.portrait;
        Instance.abilityBar1.TextureUnder = body.ability1;
        Instance.abilityBar2.TextureUnder = body.ability2;
        Instance.abilityBar1.Value = 0;
        Instance.abilityBar2.Value = 0;
    }

    public static void SetAutodamageCountdownValue(float value)
    {
        Instance.autodamageCountdown.Value = value;
    }


    public static void TriggerCooldown(int abilityBar, float cd)
    {


        TextureProgressBar bar = abilityBar == 1 ? Instance.abilityBar1 : abilityBar == 2 ? Instance.abilityBar2 : null;
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
