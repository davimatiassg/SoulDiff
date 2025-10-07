using Godot;
using System;

public partial class MeleeAIController : AnyController
{
    [Export] public NodePath PlayerPath;
    [Export] public float AttackRange = 64f;
    [Export] public float VisionRange = 1000f;
    [Export] public float AttackCooldown = 1.2f;
    [Export] public float AbilityCooldown = 5f;

    [Export] public float WanderRadius = 200f;
    [Export] public float WanderDuration = 3f;
    [Export] public float IdleDuration = 2f;

    private Vector2 PlayerGlobalPosition { get => GameManager.instance.player.currentBody.GlobalPosition; }
    private float _attackTimer = 0f;
    private float _abilityTimer = 0f;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private enum AIState { Idle, Wander, Chase }
    private AIState _state = AIState.Idle;

    private float _stateTimer = 0f;
    private Vector2 _wanderTarget;

    public override void _Ready()
    {
        _rng.Randomize();
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
                if (distance < VisionRange * 0.7f && _rng.Randf() < 0.01f)
                    SetNewState(AIState.Chase);
                break;

            case AIState.Wander:
                // Anda até o ponto de destino
                moveDir = (_wanderTarget - GlobalPosition).Normalized();
                lookDir = moveDir;

                if (GlobalPosition.DistanceTo(_wanderTarget) < 10f)
                    SetNewState(AIState.Idle);

                // Chance de avistar o jogador e decidir persegui-lo
                if (distance < VisionRange && _rng.Randf() < 0.03f)
                    SetNewState(AIState.Chase);
                break;

            case AIState.Chase:
                moveDir = toPlayer.Normalized();
                lookDir = moveDir;

                // Chance de desistir de perseguir (vira wander)
                if (distance > VisionRange * 1.2f && _rng.Randf() < 0.02f)
                    SetNewState(AIState.Wander);

                // Chance de ficar cansado e parar por um tempo
                if (_stateTimer <= 0f && _rng.Randf() < 0.01f)
                    SetNewState(AIState.Idle);
                break;
        }

        // --- Controles principais ---
        LeftAxisAction.Invoke(moveDir/2);
        RightAxisAction.Invoke(moveDir);

        // --- Ataque corpo a corpo ---
        if (distance <= AttackRange && _attackTimer <= 0f)
        {
            Button1Action.Invoke(true);
            Button1Action.Invoke(false);
            _attackTimer = AttackCooldown;
        }

        // --- Habilidade esporádica ---
        if (_abilityTimer <= 0f && distance < VisionRange * 0.75f && _rng.Randf() < 0.02f)
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
                _stateTimer = IdleDuration + _rng.RandfRange(-1f, 1f);
                break;

            case AIState.Wander:
                _stateTimer = WanderDuration + _rng.RandfRange(-1f, 1f);
                Vector2 randomOffset = new Vector2(
                    _rng.RandfRange(-WanderRadius, WanderRadius),
                    _rng.RandfRange(-WanderRadius, WanderRadius)
                );
                _wanderTarget = GlobalPosition + randomOffset;
                break;

            case AIState.Chase:
                _stateTimer = _rng.RandfRange(3f, 6f); // tempo antes de possivelmente desistir
                break;
        }
    }
}
