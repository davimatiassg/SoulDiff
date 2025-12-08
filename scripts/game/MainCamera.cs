using Godot;
using System;

public partial class MainCamera : Camera2D
{
    public static MainCamera Instance;
    [Export] public NodePath TargetPath; // arraste o nó-alvo no editor
    [Export] public float SmoothSpeed = 5f; // quanto maior, mais suave


    public static Action<double> CameraMoveAction;



    public static void CameraShake(float intensity, float duration)
    {


        Tween tween = Instance.CreateTween();

        tween.TweenMethod(
            Callable.From((float i) =>
            {
                var random = Random.GenerateFloat(0, 2 * Mathf.Pi);
                float x = Mathf.Cos(random) * i;
                float y = Mathf.Sin(random) * i;
                Instance.Offset = new Vector2(x, y) * intensity;
            }),
            intensity,
            intensity / 2,
            duration
        );


        tween.TweenCallback(Callable.From(() => Instance.Offset = Vector2.Zero));
    }


    public void ChasePlayer(double delta)
    {
        if (PlayerController.Instance == null) { CameraMoveAction -= ChasePlayer; return; }
        if (PlayerController.Instance.currentBody == null) return;

        Vector2 targetPos = PlayerController.Instance.currentBody.GlobalPosition;

        GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * SmoothSpeed);
    }

    public override void _Process(double delta)
    {
        CameraMoveAction?.Invoke(delta);
        
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

        CameraMoveAction = ChasePlayer;
    }
}