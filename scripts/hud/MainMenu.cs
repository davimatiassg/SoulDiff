using System.Threading.Tasks;
using Godot;

// O 'partial' é usado porque o Godot Mono (C#) gera código para a classe
// em um arquivo separado.
public partial class MainMenu : Control
{
    private const string MainScenePath = "res://scenes/Game.tscn"; 
    
    private const string PlayButtonPath = "MainHBoxContainer/InnerLeftHBoxContainer/ButtonsVBoxContainer/PlayButton";
    private const string QuitButtonPath = "MainHBoxContainer/InnerLeftHBoxContainer/ButtonsVBoxContainer/QuitButton";


    public override void _Ready()
    {
        if (GetNodeOrNull<Button>(PlayButtonPath) is Button playButton)
        {
            playButton.Pressed += OnPlayButtonPressed;
            GD.Print("Botão 'Jogar' conectado");
        }
        else
        {
            GD.PushError($"Erro: Não foi possível encontrar o nó do botão de Jogar no caminho: {PlayButtonPath}");
        }

        if (GetNodeOrNull<Button>(QuitButtonPath) is Button quitButton)
        {
            quitButton.Pressed += OnQuitButtonPressed;
            GD.Print("Botão 'Sair' conectado");
        }
        else
        {
             GD.PushError($"Erro: Não foi possível encontrar o nó do botão de Sair no caminho: {QuitButtonPath}");
        }
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