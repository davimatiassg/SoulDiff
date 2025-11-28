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
  
        
    }



    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }

        Instance.refuseButton.Pressed += () => GameManager.OnPlayerRefuseToDie();
        Instance.refuseButton.Pressed += () => Instance.anim.Play("outro");
    }
}
