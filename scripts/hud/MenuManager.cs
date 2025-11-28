using Godot;
using System;

public partial class MenuManager : CanvasLayer
{
    public static MenuManager Instance;

    [Export]
    private AnimationPlayer anim;

    

    [Export]
    public Button refuseButton;

    [Export]
    public Button giveupButton;

    public static void PlayDeathMenu()
    {
        Instance.anim.Play("intro");
        Instance.refuseButton.Pressed += GameManager.OnPlayerRefuseToDie;
        Instance.giveupButton.Pressed += test;
        Instance.refuseButton.Pressed += () => Instance.anim.Play("outro");
        
    }


    public static void test()
    {

        GD.Print("lolkk");
    }


    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }
}
