using Godot;
using Godot.Collections;
using System;
using System.Numerics;
using System.Reflection.Metadata;

public partial class CardManager : Node2D
{
	Godot.Vector2 screenSize = Godot.Vector2.Zero; // Get the size of the viewport for boundary checks
	Boolean isDraggingCard = false; // Local variable to track if a card is being dragged
	Node2D selectedCard = null; // Reference to the card being clicked
	Node2D cardNode; // Reference to the parent node containing all card nodes
	public Signal cardEntered; // Signal to indicate when the mouse enters a card area
	public Signal cardExited; // Signal to indicate when the mouse exits a card area
	Boolean isMouseOverCard = false; // Local variable to track if the mouse is currently over a card

	private CardTableau originalTableau= null;


    public override void _UnhandledInput(InputEvent @event)
    {
		// Check if the input event is specfically a mouse button event
       if (@event is InputEventMouseButton mouseEvent)
		{
			//Ensure we are only looking at the Left Mouse Button
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.IsPressed()){
					
					GD.Print("Left Button Pressed");
					// Perform a raycast at the mouse position to check for card interaction
					RaycastCheckForCard();
					if (selectedCard != null)
					{
						StartDraggingCard(selectedCard); // Start dragging the card if one was selected
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
		parameters.CollideWithAreas = true; // We want to detect areas (cards)
		parameters.CollisionMask = 1; // Assuming cards are on layer 1

		//Perform the intersection query
		var result = spaceRid.IntersectPoint(parameters);
		// Check if we hit something
		if (result.Count > 0)
		{	
			//Explicitly cast the first result to a Godot Dictionary
			selectedCard = GetCardWithHighestZIndex(result); // Get the card with the highest Z-index from the raycast results

			
			if (selectedCard != null)			{
				GD.Print("Top Card Clicked: " + selectedCard.Name);
				// Set the selected card reference to the collider's parent (assuming the card is the parent of the Area2D)
				
			}
			
		}
		else{
			GD.Print("No card detected at mouse position.");
			selectedCard = null; // Clear the selected card reference if no card was detected
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
		parameters.CollisionMask = 2; // Assuming card slots are on layer 2

		//Perform the intersection query
		var result = spaceRid.IntersectPoint(parameters);
		// Check if we hit something
		if (result.Count > 0)
		{	
			var hitData = (Godot.Collections.Dictionary)result[0]; // Explicitly cast the first result to a Godot Dictionary
			var collider = hitData["collider"].As<Node2D>(); // Get the collider from the hit data

			if (collider != null)
			{
				GD.Print("Card Slot Detected: " + collider.Name);
				return collider; // Return the detected card slot
			}
			
		}
		
		return null; // Return null if no card slot was detected at the mouse position
		
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
		parameters.CollisionMask = 1;

		//Ignore card we are currently dragging
		var draggedArea = selectedCard.GetNode<Area2D>("Area2D");
		parameters.Exclude = new Godot.Collections.Array<Rid> {draggedArea.GetRid()};

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


	private bool IsRed(CardSuit suit)
	{
		return suit == CardSuit.Hearts || suit == CardSuit.Diamonds;
	}

	private bool IsValidMove(Card draggedCard, Card targetCard)
	{
		//Check if suits are opposite
		bool isDifferentColor = IsRed(draggedCard.Suit) != IsRed(targetCard.Suit);
		//Check if target is exactly one rank higher than dragged card
		bool isOneRankHigher = (int)targetCard.Rank == (int)draggedCard.Rank + 1;

		return isDifferentColor && isOneRankHigher;
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
		if (selectedCard != null)
		{
			HighlightCard(selectedCard, true); // Highlight the new card that is now being hovered
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

	private void StartDraggingCard(Node2D card)
	{
		isDraggingCard = true; // Set the dragging flag to true
		selectedCard = card; // Store a reference to the card being dragged

		originalTableau = selectedCard.GetParent() as CardTableau;

		// Optionally, you can add logic here to change the card's appearance while dragging (e.g., make it semi-transparent)
	    selectedCard.Scale = new Godot.Vector2(1,1); // Ensure the card is at its normal scale when dragging starts

	}

	private void StopDraggingCard()
	{
		if (selectedCard == null)
		{
			GD.Print("Error: StopDraggingCard called but no card is currently selected.");
			return; // Exit the method if there is no selected card to stop dragging
		}
		//Reset Visual effects from dragging
		
		selectedCard.Scale = new Godot.Vector2(1.0f, 1.0f); // Reset the card's scale when dropping

		//Check if dropped card ontop of another card
		Card targetCard = GetTargetCardUnderMouse();
		//Check if dropped on an empty slot
		Node2D cardSlotFound = RaycastCheckForCardSlot(); // Check if the card is being dropped over a valid card slot
		
		if (targetCard !=null)
		{
			GD.Print($"Dropped {selectedCard.Name} onto {targetCard.Name}");
			//Solitaire Rule Check
			if (IsValidMove(selectedCard as Card, targetCard))
			{
				GD.Print("Valid Move!");
				//Get the specific tableau that the target card belongs to 
				CardTableau targetTableau = targetCard.GetParent() as CardTableau;

				if (targetTableau != null && originalTableau !=null)
				{
					//Remove from the old column
					originalTableau.RemoveCardFromTableau(selectedCard as Card);

					//Add to the new column
					targetTableau.AddCardToTableau(selectedCard as Card);
				}
			}
			else
			{
				GD.Print("Invalid Move. Returning card to original position...");
				//Snap Back Logic: Original tableau to recalculate its layout
				 originalTableau.UpdateCardTableau();
			}
		}
		else if (cardSlotFound != null)
		{
			//Hit an empty slot
			GD.Print("Card dropped on slot: " + cardSlotFound.Name);
			// Implement logic to snap the card to the slot's position or parent it to the slot
			isDraggingCard = false; // Reset the dragging flag

			selectedCard.GlobalPosition = cardSlotFound.GlobalPosition; // Snap the card to the center of the slot
			selectedCard.GetNode<Area2D>("Area2D").GetChild<CollisionShape2D>(0).Disabled=true;// Disable the card's collision shape to prevent further interactions while it's in the slot	
			//selectedCard.GetNode<Area2D>("Area2D").CollisionLayer = 0; // Disable the card's collision layer to prevent further interactions while it's in the slot
			CardSlot slotScript = cardSlotFound as CardSlot;
			
			if (slotScript != null)
			{
				GD.Print("Card slot script found on " + cardSlotFound.Name);
				slotScript.cardInSlot = true; // Set the cardInSlot variable in the CardSlot script to true to indicate that a card is now in the slot
			}	
		}
		else
		{
			GD.Print("Card dropped outside of any slot. Snapping Back.");
			// Implement logic to return the card to its original position
			if (originalTableau != null)
                {
                    originalTableau.UpdateCardTableau();
                }
		}
		isDraggingCard = false;
		selectedCard = null; // Clear the reference to the selected card
		originalTableau = null; //Clear out memory for next drag.
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isDraggingCard)
		{
			
			// Update card position to follow the mouse cursor
			var mousePosition = GetGlobalMousePosition();
			selectedCard.GlobalPosition = mousePosition.Clamp(Godot.Vector2.Zero, screenSize); // Clamp the position to stay within the screen bounds
		}
	}
}
	