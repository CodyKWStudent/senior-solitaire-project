using Godot;
using System;
using System.Reflection.Metadata;

public enum CardSuit { Hearts, Diamonds, Clubs, Spades }
public enum CardRank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
public partial class Card : Node2D
{
	// --- Data Properties ---
    public CardSuit Suit { get; private set; }
    public CardRank Rank { get; private set; }
    public bool IsFaceUp { get; private set; } = false;

	// --- Node References ---
    private Area2D cardArea;
    private Sprite2D cardSprite;

	// --- Textures ---
	private Texture2D faceTexture;
	private Texture2D backTexture;

	// --- Signals ---
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
			
			EmitSignal(SignalName.CardEntered, this); // Emit the cardEntered signal when the mouse enters the area
		};
		cardArea.MouseExited += () =>
		{
			GD.Print("Mouse exited " + Name + "'s area!");
			
			EmitSignal(SignalName.CardExited, this); // Emit the cardExited signal when the mouse exits the area
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// --Setup Cards--
	public void InitializeCard(CardSuit suit, CardRank rank)
	{
		Suit = suit;
		Rank = rank;
		Name = $"{Suit}_{Rank}";
		//Grab Sprite
		cardSprite = GetNode<Sprite2D>("Sprite2D");
		//Load Card Back Texture
		backTexture = GD.Load<Texture2D>("res://assets/cards/4x/CardBack.png");

		// Dynamically load the correct texture based on the Suit and Rank
		string texturepath = $"res://assets/cards/4x/{Suit}/{Suit}_{Rank}.png";
		
		
		//Check if the file exists before load to prevent crashes
		if (ResourceLoader.Exists(texturepath))
		{
			
			faceTexture = GD.Load<Texture2D>(texturepath);
			
			
		}
		else
		{
			GD.PrintErr($"Missing texture for {Name} at path: {texturepath}");
		}
		UpdateVisuals();
	}
	public void Flip(bool faceUp)
	{
		IsFaceUp = faceUp;
		UpdateVisuals();
	}
	private void UpdateVisuals()
	{
		//Swap sprite texture depending on face-up state
		if (cardSprite != null)
		{
			
			cardSprite.Texture = IsFaceUp ? faceTexture : backTexture;
		}
	}

}
