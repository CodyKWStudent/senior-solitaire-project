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
    
    PackedScene CARD_SCENE = (PackedScene)GD.Load("res://Scenes/Card.tscn");
    PackedScene TABLEAU_SCENE = (PackedScene)GD.Load("res://Scenes/CardTableau.tscn");
	
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
		for (int i = 0; i < 7; i++)
        {
            CardTableau newTableau = TABLEAU_SCENE.Instantiate<CardTableau>();
            newTableau.Name = $"Tableau_{i}";
			newTableau.initialTableauSize= newTableau.initialTableauSize-i;
			            
            // Space each column by 100 pixels on the X axis (adjust as needed for your game)
            newTableau.Position = new Godot.Vector2(i * 150, 0); 
            
            AddChild(newTableau);
            tableaus[i] = newTableau;
        }
	}
	
	public void DealStartingBoard()
	{
		for (int col = 0; col <= 7; col++)
		{
			for(int row = 0; row <= col; row++)
			{
				if (drawPile.Count>0)
				{
					Card dealtCard = drawPile.Pop();
				}
			}
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

}
