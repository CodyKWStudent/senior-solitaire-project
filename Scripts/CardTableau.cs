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
	private List<Card> cardsInColumn = new List<Card>();
	//Load Card from Card Scene
	PackedScene CARD_SCENE_PATH = (PackedScene)GD.Load("res://Scenes/Card.tscn");
	// Create Cardmanager Node
	SeniorSolitaireProject.Scripts.CardManager cardManager;
	//Find the starting position of the node to create the tableau column
	Vector2 tableauPosition;
	
	// Slot that appears when the tableau is empty
    private CardSlot _freeCellSlot;
    private static readonly PackedScene CardSlotScene = GD.Load<PackedScene>("res://Scenes/CardSlot.tscn");
	
	public override void _Ready()
	{
		EnsureSlotExists();
	}

    private void EnsureSlotExists()
    {
        if (_freeCellSlot == null)
        {
            _freeCellSlot = CardSlotScene.Instantiate<CardSlot>();
            // The suit doesn't matter for a FreeCell type slot.
            _freeCellSlot.InitializeCardSlot(SlotType.FreeCell, CardSuit.Clubs);
            AddChild(_freeCellSlot);

            // Position it at the tableau's origin.
            _freeCellSlot.Position = Vector2.Zero;

            // Set initial visibility.
            _freeCellSlot.Visible = IsEmpty();
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	//Create the Tableau Column and position cards below one another
	public void AddCardToTableau(Card card)
	{
        EnsureSlotExists();
		// Hide the free cell slot since the column is no longer empty.
        _freeCellSlot.Visible = false;
		
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

			// If it's the last card and face down, flip it up
			if (i == cardsInColumn.Count - 1 && !currentCard.IsFaceUp)
			{
				currentCard.Flip(true);
			}

			// Determine Interactability: Cards are interactable if they are face up
			SetCardInteractable(currentCard, currentCard.IsFaceUp);
		}
	}
	private void SetCardInteractable(Card card, bool isInteractable)
	{
		// Safely turn the collision shape on or off
		var area = card.GetNodeOrNull<Area2D>("Area2D");
		if (area != null)
		{
			var collisionShape = area.GetChild<CollisionShape2D>(0);
			if (collisionShape != null)
			{
				collisionShape.SetDeferred("disabled", !isInteractable);
			}
		}
	}

	public List<Card> GetCardsFrom(Card clickedCard)
	{
		//Create temp list to put cards into if the user attempts to drag multiple cards
		List<Card> draggedStack = new List<Card>();
		//Find where the user clicks the card and drag everything below that if applicable. 
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
			
			// If the column is now empty, show the free cell slot.
            if (IsEmpty())
            {
                EnsureSlotExists();
                _freeCellSlot.Visible = true;
            }
		}
	}

	public bool IsStackValid(Card clickedCard)
	{
		int startIndex = cardsInColumn.IndexOf(clickedCard);
		//Safety check
		if (startIndex == -1 || !clickedCard.IsFaceUp) return false;
		
		//If very last card in the column it's always valid to pick up
		if (startIndex == cardsInColumn.Count - 1) return true;

		//Loop through the stack starting from clicked card
		for (int i = startIndex; i < cardsInColumn.Count - 1; i++)
		{
			Card currentCard = cardsInColumn[i];
			Card cardBelowIt = cardsInColumn[i + 1];

			if (!cardBelowIt.IsFaceUp) return false;

			//Solitaire Rules: The card below must be opposite color and exactly 1 rank lower
			bool currentIsRed = currentCard.Suit == CardSuit.Hearts || currentCard.Suit == CardSuit.Diamonds;
			bool belowIsRed = cardBelowIt.Suit == CardSuit.Hearts || cardBelowIt.Suit == CardSuit.Diamonds;
			
			bool isDifferentColor = currentIsRed != belowIsRed;
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
