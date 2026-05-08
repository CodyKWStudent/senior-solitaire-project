using Godot;
using System;
using System.Collections.Generic;

public partial class Move : Node
{
	public List<Card> CardsMoved { get; set; }
	public Node Source { get; set; }
	public Node Destination { get; set; }
	
}
