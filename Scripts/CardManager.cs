using System;
using System.Collections.Generic;
using Godot;

namespace SeniorSolitaireProject.Scripts;

public partial class  CardManager : Node2D
{
	// --- Variables --- 
	Godot.Vector2 screenSize = Godot.Vector2.Zero; // Get the size of the viewport for boundary checks
	
	
	
	// --- Data Tracking ---
	private List<Card> draggedCards = new List<Card>(); //The whole stack being moved. 
	private CardTableau originalTableau = null;
	private CardSlot originalSlot = null;
	private Vector2 originalCardPosition;
	Node2D cardNode; // Reference to the parent node containing all card nodes
	private Card rootDraggedCard = null; // Reference to the card being clicked
	private Boolean isDraggingCard = false; // Local variable to track if a card is being dragged
	
	// --- Signals ---
	private Signal cardEntered; // Signal to indicate when the mouse enters a card area
	private Signal cardExited; // Signal to indicate when the mouse exits a card area
	private Boolean isMouseOverCard = false; // Local variable to track if the mouse is currently over a card

	
	// --- External Classes ---
	[Export] public Deck GameDeck; 


	public override void _UnhandledInput(InputEvent @event)
	{
		// Check if the input event is specifically a mouse button event
		if (@event is InputEventMouseButton mouseEvent)
		{
			//Ensure we are only looking at the Left Mouse Button
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.IsPressed()){
					
					GD.Print("Left Button Pressed");
					
					if (RaycastCheckForDeck())
					{
						GD.Print("Deck Clicked!");
						if (GameDeck != null) GameDeck.DrawCardFromDeck(3);
						return; //Stop running to not accidentally drag any cards
					}
					
					// Perform a raycast at the mouse position to check for card interaction
					RaycastCheckForCard();
					if (rootDraggedCard != null)
					{
						StartDraggingCard(rootDraggedCard); // Start dragging the card if one was selected
					}
				}
				else
				{
					GD.Print("Left Button Released");
					// Drop Logic Here
					if (isDraggingCard)
					{
						StopDraggingCard(); // Stop dragging the card when the left mouse button is released
					}
				}
			}
		}
	}

	/*
	 * This method performs a raycast at the current mouse position to check for card interactions.
	 * It uses the 2D physics engine to detect if any card (Area2D) is under the mouse cursor when the left mouse button is pressed.
	 * If a card is detected, it returns true; otherwise, it returns false.
	 */
	 
	private void RaycastCheckForCard()
	{
		// Get the current 2D physics world state
		var spaceRid = GetWorld2D().DirectSpaceState;

		// Create and configure the query parameters for the raycast
		var parameters = new PhysicsPointQueryParameters2D();
		parameters.Position = GetGlobalMousePosition(); // Start the raycast from the mouse position
		parameters.CollideWithAreas = true; // We want to detect areas 
		parameters.CollisionMask = 2; // Cards are on Layer 2

		//Perform the intersection query
		var result = spaceRid.IntersectPoint(parameters);
		// Check if we hit something
		if (result.Count > 0)
		{	
			//Explicitly cast the first result to a Godot Dictionary
			Node2D topNode = GetCardWithHighestZIndex(result); // Get the card with the highest Z-index from the raycast results
			rootDraggedCard = topNode as Card;
			
			if (rootDraggedCard != null)
			{
				var parent = rootDraggedCard.GetParent();
				originalTableau = parent as CardTableau;
				originalSlot = parent as CardSlot;

				if (originalTableau != null)
				{
					if (originalTableau.IsStackValid(rootDraggedCard))
					{
						GD.Print("Valid Stack Clicked: " + rootDraggedCard.Name);
						originalSlot = null; // Not a slot drag
						StartDraggingCard(rootDraggedCard);
					}
					else
					{
						GD.Print("Invalid Sequence. Cannot drag this sub-stack.");
						rootDraggedCard = null;
					}
				}
				else if (originalSlot != null)
				{
					GD.Print("Valid Card From Slot Clicked: " + rootDraggedCard.Name);
					originalTableau = null; // Not a tableau drag
					StartDraggingCard(rootDraggedCard);
				}
				else // Not in a tableau or slot, so it's a waste card
				{
					GD.Print("Valid Card From Waste Clicked: " + rootDraggedCard.Name);
					StartDraggingCard(rootDraggedCard);
				}
			}
			
		}
		else{
			GD.Print("No card detected at mouse position.");
			rootDraggedCard = null; // Clear the selected card reference if no card was detected
		}
	
	}

	private Node2D RaycastCheckForCardSlot()
	{
		// Get the current 2D physics world state
		var spaceRid = GetWorld2D().DirectSpaceState;

		// Create and configure the query parameters for the raycast
		var parameters = new PhysicsPointQueryParameters2D();
		parameters.Position = GetGlobalMousePosition(); // Start the raycast from the mouse position
		parameters.CollideWithAreas = true; // We want to detect areas (card slots)
		parameters.CollisionMask = 4; // Card Slots (Foundations) are on Layer 4

		//Perform the intersection query
		var result = spaceRid.IntersectPoint(parameters);
		// Check if we hit something
		if (result.Count > 0)
		{	
			var hitData = (Godot.Collections.Dictionary)result[0]; // Explicitly cast the first result to a Godot Dictionary
			var collider = hitData["collider"].As<Node2D>(); // Get the collider from the hit data
			return collider.GetParent() as CardSlot;
			
		}
		
		return null; // Return null if no card slot was detected at the mouse position
		
	}

	private bool RaycastCheckForDeck()
	{
		GD.Print("Checking for Deck");
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;
		
		parameters.CollisionMask = 3; //Deck is on Layer 3
		
		var result = spaceState.IntersectPoint(parameters);
		
		if (result.Count > 0)
		{
			var hitData = (Godot.Collections.Dictionary)result[0];
			var collider = hitData["collider"].As<Node2D>();
			if (collider != null && collider.GetParent().Name == "Deck")
			{
				return true;
			}
		}
		
		return false;
	}

	private Node2D GetCardWithHighestZIndex(Godot.Collections.Array<Godot.Collections.Dictionary> raycastResults)
	{
		Node2D topCard = null;
		int highestZIndex = int.MinValue;

		foreach (var result in raycastResults)
		{
			var hitData = (Godot.Collections.Dictionary)result;
			var collider = hitData["collider"].As<Node2D>();

			if (collider != null)
			{
				var cardNode = collider.GetParent() as Node2D; // Assuming the card is the parent of the Area2D
				if (cardNode != null && cardNode.ZIndex > highestZIndex)
				{
					highestZIndex = cardNode.ZIndex;
					topCard = cardNode;
				}
			}
		}

		return topCard;
	}
	
	private Card GetTargetCardUnderMouse()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;
		parameters.CollisionMask = 2; // Cards should be on Layer 2

		// Ignore all cards currently being dragged
		var excludeRids = new Godot.Collections.Array<Rid>();
		foreach (var card in draggedCards)
		{
			var draggedArea = card.GetNode<Area2D>("Area2D");
			if (draggedArea != null)
			{
				excludeRids.Add(draggedArea.GetRid());
			}
		}
		parameters.Exclude = excludeRids;

		var result = spaceState.IntersectPoint(parameters);

		if (result.Count > 0)
		{
			//Reuse GetCard
			Node2D topNode = GetCardWithHighestZIndex(result);

			//Cast to Card Object to read Suit and Rank later
			return topNode as Card;
		}

		return null; //Return if dropped into empty space.

	}


	public bool IsRed(CardSuit suit)
	{
		return suit is CardSuit.Hearts or CardSuit.Diamonds;
	}

	private bool IsValidMove(Card draggedCard, Card targetCard)
	{
		// Tableau stacking rule: opposite colors, and descending rank.
		bool isDifferentColor = IsRed(draggedCard.Suit) != IsRed(targetCard.Suit);
		// e.g., dragged 5 is placed on target 6. dragged.Rank + 1 == target.Rank
		bool isCorrectRank = (int)draggedCard.Rank + 1 == (int)targetCard.Rank;

		return isDifferentColor && isCorrectRank;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get the size of the viewport to use for boundary checks when dragging cards
		screenSize = GetViewport().GetVisibleRect().Size;
		foreach (Node2D child in GetChildren())
		{
			if (child is Card cardInstance)
			{
				cardInstance.CardEntered += OnCardEntered; // Connect the cardEntered signal to the OnCardEntered method
				cardInstance.CardExited += OnCardExited; // Connect the cardExited signal to the On
			}
		}

	}

	private void OnCardEntered(Card hoveredCard)
	{
		GD.Print("CardManager detected mouse entered on card: " + hoveredCard.Name);
		// You can add additional logic here to respond to the card being hovered, such as highlighting the card or showing a tooltip
		if (!isMouseOverCard) // Check if the mouse is not already over a card to prevent multiple highlights
		{
			isMouseOverCard = true; // Set the flag to indicate that the mouse is now over a card
			HighlightCard(hoveredCard, true); // Call a method to visually highlight the card (you can implement this method as needed)
		}
		
	}

	private void OnCardExited(Card exitedCard)
	{
		GD.Print("CardManager detected mouse exited from card: " + exitedCard.Name);
		// You can add additional logic here to respond to the card being unhovered
		HighlightCard(exitedCard, false);
		//Check if hovered off card straight onto another card
		RaycastCheckForCard();
		if (rootDraggedCard != null)
		{
			HighlightCard(rootDraggedCard, true); // Highlight the new card that is now being hovered
		}
		else
		{
			isMouseOverCard = false; // Reset the flag if the mouse is not over any card
		}

	}

	private void HighlightCard(Node2D cardToHighlight, bool highlight)
	{
		// Implement your logic to visually highlight the card (e.g., change its color, add a border, etc.)
		if (highlight)
		{
			// Example: Change the card's modulate color to indicate it's highlighted
			cardToHighlight.Scale = new Godot.Vector2(1.1f, 1.1f); // Slightly enlarge the card for emphasis
		}
		else
		{
			cardToHighlight.Scale = new Godot.Vector2(1, 1); // Reset scale to original size
		}

	}

	private void StartDraggingCard(Card card)
	{
		isDraggingCard = true;
		rootDraggedCard = card;

		if (originalTableau != null)
		{
			// Drag the whole stack
			draggedCards = originalTableau.GetCardsFrom(rootDraggedCard);
			//Optional make them all transparent and pop to the front
			foreach(Card c in draggedCards)
			{
				c.Modulate = new Color (1,1,1, 0.5f);
				c.ZIndex += 100; //Force them to draw oever everything else while dragging. 
			}
		}
		else // For single card drags from Slot or Waste
		{
			draggedCards = new List<Card> {rootDraggedCard};
			originalCardPosition = rootDraggedCard.Position;
			rootDraggedCard.Modulate = new Color (1,1,1, 0.5f);
			rootDraggedCard.ZIndex += 100;
		}		
	}

	private void StopDraggingCard()
	{
		if (rootDraggedCard == null)
		{
			GD.Print("Error: StopDraggingCard called but no card is currently selected.");
			return;
		}

		// Reset visual effects from dragging
		foreach (Card c in draggedCards)
		{
			c.Modulate = new Color(1, 1, 1, 1);
			c.ZIndex -= 100;
		}

		Card targetCard = GetTargetCardUnderMouse();
		CardSlot slotNode = RaycastCheckForCardSlot() as CardSlot;

		// --- Main Logic Branching ---
		if (targetCard != null)
		{
			// --- SCENARIO A: Dropped on another card ---
			CardSlot parentSlot = targetCard.GetParent() as CardSlot;
			CardTableau parentTableau = targetCard.GetParent() as CardTableau;

			if (parentSlot != null && parentSlot.SlotType == SlotType.Foundation)
			{
				HandleFoundationDrop(parentSlot);
			}
			else if (parentTableau != null)
			{
				HandleTableauDrop(parentTableau, targetCard);
			}
			else
			{
				GD.Print("Invalid card drop target. Snapping back.");
				SnapCardBack();
			}
		}
		else if (slotNode != null)
		{
			// --- SCENARIO B: Dropped on an empty slot ---
			if (slotNode.SlotType == SlotType.Foundation)
			{
				HandleFoundationDrop(slotNode);
			}
			else if (slotNode.SlotType == SlotType.Deck)
			{
				HandleDeckFreeCellDrop(slotNode);
			}
			else // Must be a FreeCell slot in an empty tableau
			{
				CardTableau emptyTableau = slotNode.GetParent() as CardTableau;
				if (emptyTableau != null)
				{
					HandleEmptyTableauDrop(emptyTableau);
				}
				else
				{
					GD.Print("Invalid empty slot drop. Snapping Back.");
					SnapCardBack();
				}
			}
		}
		else
		{
			// --- SCENARIO C: Dropped outside of anything ---
			GD.Print("Card dropped outside of any valid area. Snapping Back.");
			SnapCardBack();
		}

		// Reset state variables
		isDraggingCard = false;
		rootDraggedCard = null;
		originalTableau = null;
		originalSlot = null;
		draggedCards.Clear();
	}
	
	private void HandleFoundationDrop(CardSlot foundationSlot)
	{
		if (draggedCards.Count > 1)
		{
			GD.Print("Cannot drop multiple cards onto a Foundation.");
			SnapCardBack();
			return;
		}

		if (foundationSlot.IsValidDrop(rootDraggedCard))
		{
			GD.Print("Valid Foundation Move!");
			RemoveCardFromOriginalLocation();
			foundationSlot.AddCard(rootDraggedCard);
		}
		else
		{
			GD.Print("Invalid Foundation Drop. Snapping Back.");
			SnapCardBack();
		}
	}
	
	private void HandleDeckFreeCellDrop(CardSlot deckSlot)
	{
		if (draggedCards.Count > 1)
		{
			GD.Print("Cannot drop multiple cards onto the Deck slot.");
			SnapCardBack();
			return;
		}

		if (deckSlot.IsValidDrop(rootDraggedCard))
		{
			GD.Print("Valid Deck FreeCell Move!");
			RemoveCardFromOriginalLocation();
			deckSlot.AddCard(rootDraggedCard);
		}
		else
		{
			GD.Print("Invalid Deck FreeCell Drop. Snapping Back.");
			SnapCardBack();
		}
	}

	private void HandleTableauDrop(CardTableau targetTableau, Card targetCard)
	{
		if (IsValidMove(rootDraggedCard, targetCard))
		{
			GD.Print("Valid Tableau Move!");
			RemoveCardFromOriginalLocation();
			foreach (Card c in draggedCards)
			{
				targetTableau.AddCardToTableau(c);
			}
		}
		else
		{
			GD.Print("Invalid Tableau Move. Returning card to original position...");
			SnapCardBack();
		}
	}

	private void HandleEmptyTableauDrop(CardTableau targetTableau)
	{
		GD.Print("Card dropped on an empty Tableau column");
		RemoveCardFromOriginalLocation();
		foreach (Card c in draggedCards)
		{
			targetTableau.AddCardToTableau(c);
		}
	}

	private void RemoveCardFromOriginalLocation()
	{
		if (originalTableau != null)
		{
			foreach (var card in draggedCards)
			{
				originalTableau.RemoveCardFromTableau(card);
			}
		}
		else if (originalSlot != null)
		{
			originalSlot.RemoveCard(rootDraggedCard);
		}
		else // From waste
		{
			GameDeck.RemoveCardFromDeck(rootDraggedCard);
		}
	}

	private void SnapCardBack()
	{
		if (originalTableau != null)
		{
			originalTableau.UpdateCardTableau();
		}
		else if (originalSlot != null)
		{
			rootDraggedCard.Position = Godot.Vector2.Zero;
		}
		else // Waste card
		{
			rootDraggedCard.Position = originalCardPosition;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isDraggingCard && rootDraggedCard != null)
		{
			
			// Move the root card to the mouse
			var mousePosition = GetGlobalMousePosition();
			rootDraggedCard.GlobalPosition = mousePosition.Clamp(Godot.Vector2.Zero, screenSize); // Clamp the position to stay within the screen bounds

			// Move the rest of the stack relative to the root card.
			for (int i = 1; i < draggedCards.Count; i++)
			{
				Card trailingCard = draggedCards[i];
				trailingCard.GlobalPosition = rootDraggedCard.GlobalPosition + new Godot.Vector2(0, i * 30);
			}
		}
	}
}
