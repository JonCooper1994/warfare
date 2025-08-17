using Godot;
using System;
using System.Collections.Generic;
using System.Net;

public class Order
{

	public Move Move;

	public Dictionary<LocalDirection, bool> cannonsFired = new() {
		{LocalDirection.Forward, false},
		{LocalDirection.Left, false},
		{LocalDirection.Right, false},
		{LocalDirection.Back, false},
	};

	public static Order Forward(){
		var o = new Order();
		o.Move = Move.Forward;

		return o;
	}

	public static Order Left(){
		var o = new Order();
		o.Move = Move.Left;

		return o;
	}

	public static Order Right(){
		var o = new Order();
		o.Move = Move.Right;

		return o;
	}

	public static Order Skip(){
		var o = new Order();
		o.Move = Move.Skip;

		return o;
	}
}
