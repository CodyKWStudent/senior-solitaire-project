using Godot;
using System;
using System.Reflection.Metadata;

public partial class Card : Node2D
{
	Area2D cardArea;
	[Signal]
    public delegate void CardEnteredEventHandler(Card cardInstance); 
    
    [Signal]
    public delegate void CardExitedEventHandler(Card cardInstance);	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cardArea = GetNode<Area2D>("Area2D");
		cardArea.MouseEntered += () =>
		{
			GD.Print("Mouse entered " + Name + "'s area!");
			// You can add your logic here, such as changing the cursor or showing a tooltip
			EmitSignal(SignalName.CardEntered, this); // Emit the cardEntered signal when the mouse enters the area
		};
		cardArea.MouseExited += () =>
		{
			GD.Print("Mouse exited " + Name + "'s area!");
			// You can add your logic here, such as resetting the cursor or hiding a tooltip
			EmitSignal(SignalName.CardExited, this); // Emit the cardExited signal when the mouse exits the area
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}
