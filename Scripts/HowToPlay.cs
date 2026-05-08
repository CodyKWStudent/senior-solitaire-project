using Godot;

namespace SeniorSolitaireProject.Scripts;

public partial class HowToPlay : Node2D
{
	private Window _howToPlayWindow;
	private ColorRect menuCover;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_howToPlayWindow = GetNode<Window>("HowToPlayWindow");
		menuCover = GetNode<ColorRect>("MenuCover");
		menuCover.Visible = false;
		_howToPlayWindow.Hide();
		_howToPlayWindow.CloseRequested += CloseHowToPlayWindow;
	}

	public void ShowHowToPlayWindow()
	{
		_howToPlayWindow.Show();
		GetTree().Paused = true;
		menuCover.Visible = true;
	}

	public void CloseHowToPlayWindow()
	{
		GetTree().Paused = false;
		_howToPlayWindow.Hide();
		menuCover.Visible = false;
	}
	

}