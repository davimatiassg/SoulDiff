using System.Threading.Tasks;
using Godot;

// O 'partial' é usado porque o Godot Mono (C#) gera código para a classe
// em um arquivo separado.
public partial class MainMenu : Control
{
    private const string MainScenePath = "res://scenes/Game.tscn"; 
    
    [Export] private TextureButton playButton;
    [Export] private TextureButton quitButton;


    public override void _Ready()
    {
        playButton.Pressed += OnPlayButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        SceneManager.ChangeScene("res://scenes/Game.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GD.Print("Fechando Jogo...");
      
        GetTree().Quit();
    }
}