using Godot;
using System;

namespace SeniorSolitaireProject.Scripts;

public partial class HeaderMenu : Node
{
	private const string MainGameScenePath = "res://Scenes/Main.tscn";
	
	private Label timeNumber;
	private Label turnNumber;
	private Button newDeal;
	private Button quit;
	private Button undo;
	private Button win;
	private Button howToPlay;
	private int _turnCounter = 0;

	
	private float _elapsedTime = 0f;

	[Export] public GameLogic GameLogic;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timeNumber = GetNode<Label>("TimeLabel/TimeNumber");
		turnNumber = GetNode<Label>("TurnsLabel/TurnsNumber");
		newDeal = GetNode<Button>("NewDealButton");
		quit = GetNode<Button>("QuitButton");
		undo = GetNode<Button>("UndoButton");
		win = GetNode<Button>("WinGameButton");
		howToPlay = GetNode<Button>("HowToPlayButton");
		
		undo.Pressed += OnUndoPress;
		newDeal.Pressed += OnNewDealPress;
		quit.Pressed += OnQuitPress;
		win.Pressed += OnWinTestPress;
		howToPlay.Pressed += OnHowToPlayPress;
		
		UpdateTurnCounter();

	}

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_elapsedTime += (float)delta;
		
		// Format the time into minutes and seconds
		var timeSpan = TimeSpan.FromSeconds(_elapsedTime);
		// "d2" ensures the numbers are always two digits (e.g., 01, 02)
		string timeString = $"{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}"; 
		
		timeNumber.Text = timeString;
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

	private void OnUndoPress()
	{
		
		GameLogic.UndoLastMove();
	}

	private void OnWinTestPress()
	{
		GameLogic.WinTesting();
	}
	private void OnHowToPlayPress()
	{
		GameLogic.ShowHowToPlay();
	}

	public void AddTurn()
	{
		_turnCounter++;
		UpdateTurnCounter();
	}

	private void UpdateTurnCounter()
	{
		turnNumber.Text = _turnCounter.ToString();
	}

	public Label GetTime()
	{
		return timeNumber;
	}

	public Label GetTurns()
	{
		return turnNumber;
	}
}
