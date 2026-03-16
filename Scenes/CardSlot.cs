using Godot;
using System;

public partial class CardSlot : Node2D
{
	public Boolean cardInSlot = false; // Local variable to track if a card is currently in the slot
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(this.GetChild<Area2D>(0).CollisionMask);	

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
