using Godot;
using System;
using System.Collections.Generic;

public partial class CardTableau : Node2D
{
	//Amount of cards in tableau. This will change with each tableau column
	//Export to change this number in the godot inspector
	[Export]
	public int initialTableauSize = 7;
	//Y-Offset for cards overlapping
	private const float Y_OFFSET = 50.0f;
	//List to track cards in each column
	public List<Card> cardsInColumn = new List<Card>();
	//Load Card from Card Scnee
	PackedScene CARD_SCENE_PATH = (PackedScene)GD.Load("res://Scenes/Card.tscn");
	// Create Cardmanager Node
	CardManager cardManager;
	//Find the starting position of the node to create the tableau column
	Vector2 tableauPosition;
	 
	
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	//Create the Tableau Column and position cards below one another
	public void AddCardToTableau(Card card)
	{
		//Add card to list
		cardsInColumn.Add(card);
		//Add as visual child of Tableau Node
		if (card.GetParent() != this)
		{
			if (card.GetParent() != null) card.GetParent().RemoveChild(card);
			AddChild(card);
		}
		//Update visual positions and interactability of the column
		UpdateCardTableau();
	}
	//If card is moved from position make sure to move it back. 
	public void UpdateCardTableau()
	{
		for (int i = 0; i < cardsInColumn.Count; i++)
        {
            Card currentCard = cardsInColumn[i];
            
            // Snap the card to its correct overlapping position
            currentCard.Position = new Vector2(0, i * Y_OFFSET);
            
            // Ensure the Z-index stacks properly so lower cards draw on top
            currentCard.ZIndex = i;

            // Determine Interactability: Only the LAST card in the list can be clicked
            SetCardInteractable(currentCard, true);
        }
	}
	private void SetCardInteractable(Card card, bool isInteractable)
    {
        // Safely turn the collision shape on or off
        // (Assuming your Card scene has an Area2D with a CollisionShape2D)
        var collisionShape = card.GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
        
        if (collisionShape != null)
        {
            collisionShape.SetDeferred("disabled", !isInteractable);
        }
        
        // Optional: You could also add logic here to flip the card's sprite face-up or face-down!
    }

	public List<Card> GetCardsFrom(Card clickedCard)
	{
		List<Card> draggedStack = new List<Card>();
		int startIndex = cardsInColumn.IndexOf(clickedCard);

		if(startIndex != -1)
		{
			//Grab the clicked card and everything after it
			for (int i = startIndex; i < cardsInColumn.Count; i++)
			{
				draggedStack.Add(cardsInColumn[i]);
			}
		}
		return draggedStack;
	}


	
	public void RemoveCardFromTableau(Card card)
	{
		//If card is in list remove it
		if (cardsInColumn.Contains(card))
		{
			cardsInColumn.Remove(card);
			//Rerun update logic so the new bottom card becomes interactable
			UpdateCardTableau();
		}
	}

	public bool IsStackValid(Card clickedCard)
	{
		int startIndex = cardsInColumn.IndexOf(clickedCard);
		//Safety check
		if (startIndex == -1) return false;
		
		//If ver last card in the column it's always valid to pick up
		if (startIndex == cardsInColumn.Count - 1) return true;

		//Loop through the stack starting from clicked card
		for (int i = startIndex; i < cardsInColumn.Count - 1; i++)
		{
			Card currentCard = cardsInColumn[i];
			Card cardBelowIt = cardsInColumn[i + 1];

			//Null check
			if(currentCard == null || cardBelowIt == null) return false;

			//Solitaire Rules: The card below must be opposite color and exactly 1 rank lower
			bool isDifferentColor = cardManager.IsRed(currentCard.Suit) != cardManager.IsRed(cardBelowIt.Suit);
            bool isOneRankLower = (int)cardBelowIt.Rank == (int)currentCard.Rank - 1;

			if (!isDifferentColor || !isOneRankLower)
			{
				//Sequence broken do not grab stack.
				return false;
			}

		}
		//Finishing then sequence is true and return true.
		return true;
	}

	public bool IsEmpty()
	{
		return cardsInColumn.Count ==0;
	}

}
