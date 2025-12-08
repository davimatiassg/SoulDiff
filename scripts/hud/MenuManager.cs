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
        else if (Instance != this)
        {
            try
            {
                Instance.QueueFree();
            } catch (ObjectDisposedException e) {}
            Instance = this;
        }

        Instance.refuseButton.Pressed += () => SequenceManager.OnPlayerRefuseToDie();
        Instance.refuseButton.Pressed += () => Instance.anim.Play("outro");

        Instance.giveupButton.Pressed += () => SceneManager.ChangeScene("res://scenes/GameOver.tscn");
        Instance.giveupButton.Pressed += () => Instance.anim.Play("outro");

        
    }
}
