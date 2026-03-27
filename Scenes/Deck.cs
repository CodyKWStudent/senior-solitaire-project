using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class Deck : Node2D
{
	// A Stack is perfect for drawing cards
    Stack<Card> drawPile = new Stack<Card>();
    
    // We will keep an array of our 7 Tableaus to easily reference them
    CardTableau[] tableaus = new CardTableau[7];

	CardTableau cardTableau = new CardTableau();
    
    PackedScene CARD_SCENE = (PackedScene)GD.Load("res://Scenes/Card.tscn");
    PackedScene TABLEAU_SCENE = (PackedScene)GD.Load("res://Scenes/CardTableau.tscn");
	[Export]
	public Node2D tableauContainer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateTableaus();
		InitializeAndShuffleDeck();
		DealStartingBoard();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GenerateTableaus()
	{
		//Programmatically create the 7 columns and space them out horizontally
		for (int i = 1; i < 7; i++)
        {
            CardTableau newTableau = TABLEAU_SCENE.Instantiate<CardTableau>();
            newTableau.Name = $"Tableau_{i}";

			//Increasing tableau size from 1 to 7 in order to store initial deal. 
			newTableau.initialTableauSize = i+1;
			            
            // Space each column by X pixels on the X axis
            newTableau.Position = new Godot.Vector2(i * 150, 0); 

			if (tableauContainer != null)
			{
				tableauContainer.AddChild(newTableau);
			}
			else
			{
				GD.PrintErr("TableauContainer not assigned in Deck Inspector. Falling back to Deck Position");
				AddChild(newTableau);
			}

            tableaus[i] = newTableau;
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

		public void DealStartingBoard()
	{
		for (int col = 0; col <= 7; col++)
		{
			CardTableau currentTableau = tableaus[col];

			for(int row = 0; row <= currentTableau.initialTableauSize; row++)
			{
				if (drawPile.Count>0)
				{
					Card dealtCard = drawPile.Pop();
					currentTableau.AddCardToTableau(dealtCard);
				}
			}
		}

		
	}

}
