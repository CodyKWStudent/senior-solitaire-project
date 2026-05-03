using Godot;
using System;
using System.Collections.Generic;

namespace SeniorSolitaireProject.Scripts;

public partial class GameLogic : Node2D
{
    // --- Foundation Tracking ---
    private bool _isClubsFoundationComplete = false;
    private bool _isDiamondsFoundationComplete = false;
    private bool _isHeartsFoundationComplete = false;
    private bool _isSpadesFoundationComplete = false;

    // --- Scene Paths ---
    private const string GameEndScenePath = "res://Scenes/GameEnd.tscn";

    public override void _Ready()
    {
        // Find all foundation slots and connect to their signals
        foreach (var node in GetTree().GetNodesInGroup("Foundations"))
        {
            if (node is CardSlot slot && slot.SlotType == SlotType.Foundation)
            {
                slot.FoundationComplete += OnFoundationComplete;
                slot.FoundationIncomplete += OnFoundationIncomplete;
            }
        }
    }

    private void OnFoundationComplete(CardSuit suit)
    {
        GD.Print($"Foundation complete for {suit}");
        SetFoundationStatus(suit, true);
        CheckForWinCondition();
    }

    private void OnFoundationIncomplete(CardSuit suit)
    {
        GD.Print($"Foundation incomplete for {suit}");
        SetFoundationStatus(suit, false);
    }

    private void SetFoundationStatus(CardSuit suit, bool isComplete)
    {
        switch (suit)
        {
            case CardSuit.Clubs:
                _isClubsFoundationComplete = isComplete;
                break;
            case CardSuit.Diamonds:
                _isDiamondsFoundationComplete = isComplete;
                break;
            case CardSuit.Hearts:
                _isHeartsFoundationComplete = isComplete;
                break;
            case CardSuit.Spades:
                _isSpadesFoundationComplete = isComplete;
                break;
        }
    }

    private void CheckForWinCondition()
    {
        if (_isClubsFoundationComplete && _isDiamondsFoundationComplete &&
            _isHeartsFoundationComplete && _isSpadesFoundationComplete)
        {
            GD.Print("All foundations are complete! Game Won!");
            GameEnd();
        }
    }

    private void GameEnd()
    {
        var scene = GD.Load<PackedScene>(GameEndScenePath);
        if (scene != null)
        {
            // Pause the game and instance the GameEnd screen as a child.
            // This makes it an overlay.
            GetTree().Paused = true;
            var gameEndInstance = scene.Instantiate();
            AddChild(gameEndInstance);
        }
        else
        {
            GD.PrintErr($"Failed to load game end scene at path: {GameEndScenePath}");
        }
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }
}
