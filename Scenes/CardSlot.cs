using Godot;
using System;
using System.Collections.Generic;
public enum SlotType {Foundation, FreeCell}
public partial class CardSlot : Node2D
{
	public SlotType slotType;
    public CardSuit targetSuit; // Only matters if it's a Foundation
    
    // This list will act as our Stack
    private List<Card> stackedCards = new List<Card>();

    private Sprite2D slotSprite;
    
    // --- Signal for Winning ---
    [Signal]
    public delegate void FoundationCompleteEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(this.GetChild<Area2D>(0).CollisionMask);	
		slotSprite = GetNode<Sprite2D>("Sprite2D");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void InitializeCardSlot (SlotType type, CardSuit suit = CardSuit.Hearts)
	{
		slotType = type;
		targetSuit = suit;
		
		string texturePath = "";
		if (slotType == SlotType.Foundation)
		{
			texturePath = $"res://assets/cards/{suit}/{suit}_CardSlot.png";
		}
		else
		{
			texturePath = $"res://assets/cards/CardSlot.png";
		}
		if (ResourceLoader.Exists(texturePath))
		{
			slotSprite.Texture = GD.Load<Texture2D>(texturePath);
		}
	}

	// --- Rule Checking ---
	public bool IsValidDrop(Card card)
	{
		if (slotType == SlotType.FreeCell)
		{
			//FreeCells should only hold 1 Card. If empty, it's valid
			return stackedCards.Count == 0;
		}
		else if (slotType == SlotType.Foundation)
		{
			// Must match the slot's suit
			if (card.Suit != targetSuit) return false;
			if (stackedCards.Count == 0)
			{
				//If empty, must be an Ace 
				return card.Rank == CardRank.Ace;
			}
			else
			{
				// If not empty, must be exactly 1 rank higher than the current top card
				Card topCard = stackedCards[stackedCards.Count - 1];
				return (int)card.Rank == (int)topCard.Rank +1;
			}
		}
		return false;
	}
	public void AddCard(Card card)
	{
	// Hide the current top card before adding the new one
        if (stackedCards.Count > 0)
        {
            Card currentTop = stackedCards[stackedCards.Count - 1];
            currentTop.Visible = false;
            // Disable its collision so it can't be clicked
            currentTop.GetNode<Area2D>("Area2D").GetChild<CollisionShape2D>(0).SetDeferred("disabled", true);
        }

        stackedCards.Add(card);
        
        // Parent the card to the slot and center it
        if (card.GetParent() != null) card.GetParent().RemoveChild(card);
        AddChild(card);
        card.Position = Vector2.Zero; // Center exactly on the slot

        // Check for King to emit win signal!
        if (slotType == SlotType.Foundation && card.Rank == CardRank.King)
        {
            //EmitSignal(SignalName.FoundationComplete);
        }	
	}
	public void RemoveCard(Card card)
	{
		if (stackedCards.Contains(card))
        {
            stackedCards.Remove(card);

            // Unhide the card beneath it!
            if (stackedCards.Count > 0)
            {
                Card newTop = stackedCards[stackedCards.Count - 1];
                newTop.Visible = true;
                newTop.GetNode<Area2D>("Area2D").GetChild<CollisionShape2D>(0).SetDeferred("disabled", false);
            }
        }
	}
}
