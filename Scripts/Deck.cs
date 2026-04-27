using System;
using System.Collections.Generic;
using Godot;

namespace SeniorSolitaireProject.Scripts;

public partial class Deck : Node2D
{
	
	// --- Data Structures ---
	readonly Stack<Card> drawPile = new Stack<Card>(); // A Stack for drawing cards
	CardTableau[] tableaus = new CardTableau[7]; // An array of our 7 Tableaus to easily reference them
	CardTableau cardTableau = new CardTableau();
	public List<Card> WastePile = new List<Card>();
    
	// --- Node References --
	private Area2D deckArea;
	private Sprite2D deckSprite;
	private CardSlot _freeCellSlot;// Slot that appears when the tableau is empty
	
	// --- Loaded Scenes ---
	PackedScene CARD_SCENE = (PackedScene)GD.Load("res://Scenes/Card.tscn");
	PackedScene TABLEAU_SCENE = (PackedScene)GD.Load("res://Scenes/CardTableau.tscn");
	PackedScene CARDSLOT_SCENE = (PackedScene)GD.Load("res://Scenes/CardSlot.tscn");
	private static readonly PackedScene CardSlotScene = GD.Load<PackedScene>("res://Scenes/CardSlot.tscn");
	
	
	
	// --- Containers to spawn things ---
	[Export] public Node2D TableauContainer;
	[Export] public Node2D FoundationSlotContainer;
	[Export] public Node2D DeckDrawContainer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		deckArea = GetNode<Area2D>("Area2D");
		deckSprite = GetNode<Sprite2D>("Sprite2D");
		GenerateTableaus();
		GenerateFoundations();
		InitializeAndShuffleDeck();
		DealStartingBoard();
		EnsureSlotExists();
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void GenerateTableaus()
	{
		//Programmatically create the 7 columns and space them out horizontally
		for (int i = 0; i < 7; i++)
		{
			CardTableau newTableau = TABLEAU_SCENE.Instantiate<CardTableau>();
			newTableau.Name = $"Tableau_{i}";

			//Increasing tableau size from 1 to 7 in order to store initial deal. 
			newTableau.initialTableauSize = i+1;
			            
			// Space each column by X pixels on the X axis
			newTableau.Position = new Godot.Vector2(i * 150, 0); 

			if (TableauContainer != null)
			{
				
				TableauContainer.AddChild(newTableau);
			}
			else
			{
				GD.PrintErr("TableauContainer not assigned in Deck Inspector. Falling back to Deck Position");
				AddChild(newTableau);
			}

			tableaus[i] = newTableau;
		}
	}

	private void GenerateFoundations()
	{
		int index = 0;
		//Loop through the 4 suits 
		foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
		{
			CardSlot newCardSlot = CARDSLOT_SCENE.Instantiate<CardSlot>();
			newCardSlot.Name = $"Foundation_{suit}";
			newCardSlot.Position = new Godot.Vector2(index * 200, 0);

			//Tell the slot it is a Foundation slot and assign its specific suit
			newCardSlot.InitializeCardSlot(SlotType.Foundation, suit);

			if(FoundationSlotContainer != null)
			{
				FoundationSlotContainer.AddChild(newCardSlot);
				GD.Print($"Added {newCardSlot.Name} to the foundation container");
			}
			else
			{
				GD.PrintErr("FoundationsSlotContainer not assigned. Falling Back to Deck");
				AddChild(newCardSlot);
			}
			index++;
		}
	}
	
	private void InitializeAndShuffleDeck()
	{
		List<Card> tempDeckList = new List<Card>();

		//Generate 52 cards
		foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
		{
			foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
			{
				//Instantiate Card Scene
				Card newCard = CARD_SCENE.Instantiate<Card>();

				//Initialize with suit and rank
				newCard.InitializeCard(suit,rank);

				tempDeckList.Add(newCard);
			}
		}
		//Shuffle the list
		Random random = new Random();
		int n = tempDeckList.Count;
		while (n>1)
		{
			n--;
			int k = random.Next(n+1);
			Card value = tempDeckList[k];
			tempDeckList[k] = tempDeckList[n];
			tempDeckList[n] = value;
		}
		//Push shuffled list onto Stack
		foreach (Card card in tempDeckList)
		{
			drawPile.Push(card);
		}
		
	}

	private void DealStartingBoard()
	{
		for (int col = 0; col < 7; col++)
		{
			CardTableau currentTableau = tableaus[col];

			for(int row = 0; row < currentTableau.initialTableauSize; row++)
			{
				
				if (drawPile.Count>0)
				{
					Card dealtCard = drawPile.Pop();
					dealtCard.Flip(true);
					currentTableau.AddCardToTableau(dealtCard);
				}
			}
		}	
	}

	public void DrawCardFromDeck(int amountToDraw)
	{
		

		if (drawPile.Count == 0)
		{
			GD.Print("Deck is empty");
			return;
		}

		for (int i = 0; i < amountToDraw; i++)
		{
			if (drawPile.Count > 0)
			{
				Card drawnCard = drawPile.Pop();
				WastePile.Add(drawnCard);
				
				if (drawnCard.GetParent() != null) drawnCard.GetParent().RemoveChild(drawnCard);
				DeckDrawContainer.AddChild(drawnCard);
				
				drawnCard.Flip(true);
			}
		}

		UpdateWastePileVisuals();
		if (drawPile.Count == 0)
		{
			GD.Print("Deck is empty");
			EnsureSlotExists();
			deckSprite.Visible = false;
			_freeCellSlot.Visible = true;
			if (deckArea != null)
			{
				deckArea.SetDeferred("disabled", true);
				
			}
		}

	}

	public void RemoveCardFromDeck(Card card)
	{
		if (!WastePile.Contains(card)) return;
		WastePile.Remove(card);
		//Rerun update logic so the new bottom card becomes interactable
		UpdateWastePileVisuals();
	}

	private void UpdateWastePileVisuals()
	{
		// Loop through all drawn cards to position them and manage collisions
		for (int i = 0; i < WastePile.Count; i++)
		{
			Card c = WastePile[i];
			
			//Offset so they overlap
			c.Position = new Godot.Vector2(i * 20, 0);
			c.ZIndex = i; // Make sure newer cards draw on top
			
			//Only the last card should be interacted with. 
			bool isTopCard = (i == WastePile.Count - 1);
			
			var shape = c.GetNode<Area2D>("Area2D").GetChild<CollisionShape2D>(0);
			shape.SetDeferred("disabled", !isTopCard);
			
		}
	}
	
	private void EnsureSlotExists()
	{
		if (_freeCellSlot == null)
		{
			_freeCellSlot = CardSlotScene.Instantiate<CardSlot>();
			// The suit doesn't matter for a FreeCell type slot.
			_freeCellSlot.InitializeCardSlot(SlotType.Deck, CardSuit.Clubs);
			AddChild(_freeCellSlot);

			// Position it at the Deck's origin.
			_freeCellSlot.Position = Vector2.Zero;

			// Set initial visibility.
			_freeCellSlot.Visible = false;
		}
	}
}
