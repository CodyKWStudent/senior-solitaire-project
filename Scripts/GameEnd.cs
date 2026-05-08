using Godot;

namespace SeniorSolitaireProject.Scripts;

public partial class GameEnd : Node
{
	private Window GameEndWindow;

	private Label timeNumber;
	private Label turnsNumber;
	private MenuButton newDeal;
	private MenuButton quit;
	private ColorRect menuCover;
	
	private const string MainGameScenePath = "res://Scenes/Main.tscn";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// This node should be able to run while the game is paused.
		ProcessMode = ProcessModeEnum.Always;
		
		// The script is attached to the root node of the GameEnd scene.
		// The window is a child of this node.
		GameEndWindow = GetNode<Window>("GameEndWindow");
		//gameEndWindow.Borderless = true;
		GameEndWindow.Visible = false;
		
		menuCover = GetNode<ColorRect>("MenuCover");
		menuCover.Visible = false;
		
		timeNumber = GetNode<Label>("GameEndWindow/TimeLabel/TimeNumber");
		turnsNumber = GetNode<Label>("GameEndWindow/TurnsLabel/TurnsNumber");
		newDeal = GetNode<MenuButton>("GameEndWindow/NewDealButton");
		quit = GetNode<MenuButton>("GameEndWindow/QuitButton");
		
		newDeal.Pressed += OnNewDealPress;
		quit.Pressed += OnQuitPress;
		GameEndWindow.CloseRequested += HideWindow;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private void OnNewDealPress()
	{
		// Unpause the game before changing scenes to ensure the new scene runs correctly.
		GetTree().Paused = false;
		
		var scene = GD.Load<PackedScene>(MainGameScenePath);
		if (scene != null)
		{
			GetTree().ChangeSceneToPacked(scene);
		}
		else
		{
			GD.PrintErr($"Failed to load main game scene at path: {MainGameScenePath}");
		}
	}

	private void OnQuitPress()
	{
		GetTree().Quit();
	}
	
	public void ShowGameEndWindow(string finalTime, string finalTurns)
	{
		timeNumber.Text = finalTime;
		turnsNumber.Text = finalTurns;
		
		GameEndWindow.Show();
		menuCover.Visible = true;
	}

	private void HideWindow()
	{
		GameEndWindow.Hide();
		menuCover.Visible = false;
	}
}
