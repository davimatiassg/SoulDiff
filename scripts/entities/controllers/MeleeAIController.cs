using Godot;
using System;

[GlobalClass]
public partial class MeleeAIController : AnyController
{
    [Export] public NodePath PlayerPath;
    [Export] public float AttackRange = 64f;
    [Export] public float VisionRange = 200f;
    [Export] public float AttackCooldown = 1.2f;
    [Export] public float AbilityCooldown = 5f;

    [Export] public float WanderRadius = 192f;
    [Export] public float WanderDuration = 3f;
    [Export] public float IdleDuration = 2f;

    private Vector2 PlayerGlobalPosition { get => PlayerController.Instance.currentBody.GlobalPosition; }
    private float _attackTimer = 0f;
    private float _abilityTimer = 0f;

    private enum AIState { Idle, Wander, Chase }
    private AIState _state = AIState.Idle;

    private float _stateTimer = 0f;
    private Vector2 _wanderTarget;

    public override void _Ready()
    {
        SetNewState(AIState.Idle);
    }

    public override void _Process(double delta)
    {

        float dt = (float)delta;
        _attackTimer -= dt;
        _abilityTimer -= dt;
        _stateTimer -= dt;

        Vector2 toPlayer = PlayerGlobalPosition - GlobalPosition;
        float distance = toPlayer.Length();
        Vector2 moveDir = Vector2.Zero;
        Vector2 lookDir = Vector2.Right;

        switch (_state)
        {
            case AIState.Idle:
                // Fica parado olhando aleatoriamente
                lookDir = toPlayer.Normalized();
                if (_stateTimer <= 0f)
                    SetNewState(AIState.Wander);

                // Chance de começar a perseguir o jogador se estiver por perto
                if (distance < VisionRange * 0.7f && Random.GenerateFloat() < 0.01f)
                    SetNewState(AIState.Chase);
                break;

            case AIState.Wander:
                // Anda até o ponto de destino
                moveDir = (_wanderTarget - GlobalPosition).Normalized();
                lookDir = moveDir;

                if (GlobalPosition.DistanceTo(_wanderTarget) < 10f)
                    SetNewState(AIState.Idle);

                // Chance de avistar o jogador e decidir persegui-lo
                if (distance < VisionRange && Random.GenerateFloat() < 0.03f)
                    SetNewState(AIState.Chase);
                break;

            case AIState.Chase:
                moveDir = toPlayer.Normalized();
                lookDir = moveDir;

                // Chance de desistir de perseguir (vira wander)
                if (distance > VisionRange * 1.2f && Random.GenerateFloat() < 0.02f)
                    SetNewState(AIState.Wander);

                // Chance de ficar cansado e parar por um tempo
                if (_stateTimer <= 0f && Random.GenerateFloat() < 0.01f)
                    SetNewState(AIState.Idle);
                break;
        }

        // --- Controles principais ---
        LeftAxisAction.Invoke(moveDir);
        RightAxisAction.Invoke(moveDir);

        // --- Ataque corpo a corpo ---
        if (distance <= AttackRange && _attackTimer <= 0f)
        {
            Button1Action.Invoke(true);
            CreateTween().TweenMethod(Callable.From((int i) => { if (i == 1) Button1Action.Invoke(false); }), -1, 1, 1f);
            _attackTimer = AttackCooldown;
        }

        // --- Habilidade esporádica ---
        if (_abilityTimer <= 0f && distance < VisionRange * 0.75f && Random.GenerateFloat() < 0.02f)
        {
            Button2Action.Invoke(true);
            _abilityTimer = AbilityCooldown;
            Button2Action.Invoke(false);
        }
    }

    private void SetNewState(AIState newState)
    {
        _state = newState;

        switch (_state)
        {
            case AIState.Idle:
                _stateTimer = IdleDuration + Random.GenerateFloat(-1f, 1f);
                break;

            case AIState.Wander:
                _stateTimer = WanderDuration + Random.GenerateFloat(-1f, 1f);
                Vector2 randomOffset = new Vector2(
                    Random.GenerateFloat(-WanderRadius, WanderRadius),
                    Random.GenerateFloat(-WanderRadius, WanderRadius)
                );
                _wanderTarget = GlobalPosition + randomOffset;
                break;

            case AIState.Chase:
                _stateTimer = Random.GenerateFloat(3f, 6f); // tempo antes de possivelmente desistir
                break;
        }
    }
}
