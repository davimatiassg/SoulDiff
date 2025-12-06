using Godot;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;


public partial class GameManager : Node
{


	public static GameManager Instance;


    public static Action OnPlayerDie;

	public static Action OnPlayerRefuseToDie;

    [Export]
    public Control DeathMenu;


	[Export]
	public int deathCount = 0;

	private void PlayerFirstDeath()
	{
		OnPlayerDie = PlayerDeath;


		OnPlayerRefuseToDie = () =>
		{
			GhostSpawn();
			AudioPlayer.PlayMusic("mus_The_Hexsmith");
			//TODO! - Remover gradualmente zoom na câmera

			OnPlayerRefuseToDie = () => SceneManager.ChangeLevel("Level_1");
		};


		Instance.CreateTween().TweenProperty(AudioPlayer.Instance.musicPlayer, "pitch_scale", 0.5, 1f);
		//TODO! - Zoom na câmera
		MenuManager.PlayDeathMenu();
	
	}
	private void PlayerDeath()
	{

		if (PlayerController.Instance.currentBody is ArcherBody)
		{ GhostSpawn(); return; }
		deathCount++;
		//TODO! - Desacelerar a música
		//TODO! - Zoom na câmera
		MenuManager.PlayDeathMenu();
	}

	public void GhostSpawn()
	{
		var body = PlayerController.Instance.currentBody;
		Debug.Assert(body != null);

		PlayerController.Disembody(body);

		OnPlayerRefuseToDie = () => SceneManager.ChangeLevel("Level_1");
	}



	public override void _Ready()
	{
		base._Ready();
		if (Instance == null) { Instance = this; }
		else if (Instance != this) { QueueFree(); return; }

		OnPlayerDie += PlayerFirstDeath;
	}
}
