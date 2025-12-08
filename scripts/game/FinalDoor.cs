using Godot;
using System;

public partial class FinalDoor : Node2D
{
    [Export] Area2D triggerCollision;
    [Export] AnimatedSprite2D anim;

    [Export] public int remainingKeys = 2;

    [Export] public Sprite2D label;
    [Export] public RichTextLabel labelText;

    public void PlaceLabel(Vector2 position, string text)
    {
        label.Visible = true;
        label.GlobalPosition = position;
        labelText.Text = text;

        Tween tween = CreateTween();
        tween.TweenInterval(5);
        tween.TweenCallback(Callable.From(() => label.Visible = false));
    }
    

    public void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
	{
        GD.Print("collided");
        if (body is AnyBody entityBody && entityBody.isPlayer)
        {
            if (remainingKeys <= 0)
            {
                anim.Play("open");
                Timer.Pause();
                SceneManager.ChangeScene("res://scenes/GameWon.tscn");
            }
            else
            {
                PlaceLabel(this.GlobalPosition, $"The door is locked. [shake]{remainingKeys} Key(s) remaining.[/shake]");
                GD.Print("door still closed");
                //TODO: Show dialog 
                // "The door is locked. There are {remainingKeys} Keys left around the dungeon."
            }
        }
	}


    public void CollectKey()
    {
        remainingKeys--;
        string text = remainingKeys > 0 ?
        $"You found a Key.[color=yellow] {remainingKeys} left![/color]" :
        $"You found the last Key. [color=cyan][shake]Run to the exit![/shake][/color]";

        PlaceLabel(PlayerController.Instance.currentBody.GlobalPosition + Vector2.Up*24, text);
    }

    public void SetupDoor()
    {
        DoorKey.OnCollectKey += CollectKey;
    }
    public override void _Ready()
    {
        base._Ready();
        if (triggerCollision == null) return;
        CallDeferred(MethodName.SetupDoor);

        triggerCollision.BodyShapeEntered += OnCollisionEnter;

    }
    

}
