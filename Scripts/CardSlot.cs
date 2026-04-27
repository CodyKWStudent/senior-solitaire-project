using Godot;
using System;
using System.Collections.Generic;
public enum SlotType {Foundation, FreeCell, Deck}
public partial class CardSlot : Node2D
{
	public SlotType SlotType;
    public CardSuit TargetSuit; // Only matters if it's a Foundation
    
    // Each slot of the foundation will be a stack of cards as a list
    private List<Card> stackedCards = new List<Card>();

    private Sprite2D slotSprite;
    
    // --- Signal for Winning ---
    [Signal]
    public delegate void FoundationCompleteEventHandler();
    
	public void InitializeCardSlot (SlotType type, CardSuit suit)
	{
		SlotType = type;
		TargetSuit = suit;
		slotSprite = GetNode<Sprite2D>("Sprite2D");
		
		string texturePath = "";
		if (SlotType == SlotType.Foundation)
		{
			texturePath = $"res://assets/cards/4x/{suit}/{suit}_CardSlot.png";
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
		if (SlotType == SlotType.FreeCell)
		{
			//Free Cells support more than one card
			return true;
		}
		else if (SlotType == SlotType.Foundation)
		{
			// Must match the slot's suit
			if (card.Suit != TargetSuit) return false;
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
		else
		{
			//Deck Free Cell should only ever take exactly 1 card. Cannot have other cards move into it while stacked
			//Nor should they abe able to have cards stack into it like a foundation
			if (stackedCards.Count == 0) return true;
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
		if (slotSprite !=null) slotSprite.Visible = false;

        // Check for King to emit win signal!
        if (SlotType == SlotType.Foundation && card.Rank == CardRank.King)
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
			if (stackedCards.Count == 0 && slotSprite != null)
			{
				slotSprite.Visible = true;
			}
        }
	}
}
