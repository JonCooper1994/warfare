using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FinalGridPath
{

	public Direction startDirection;
	
	public List<Vector2I> path;

	public Vector2I finalPosition;

	public int collisionIndex = 99;

	public bool hasCollided = false;

	public FinalGridPath(Direction startDirection, List<Vector2I> path){
		this.startDirection = startDirection;
		this.path = path;
	}

	public int MaxIndex() {
		return path.Count - 1;
	}

	// Gets the position of this planned path at a specific step index
	public Vector2I PosAtStep(int step) {
		if(step > MaxIndex() ) { step = MaxIndex(); }

		return path[step];
	}

	// Removes elements *after* the collision index
	public void Collide(int step) {
		collisionIndex = step;
		hasCollided = true;

		path = path.Take(step).ToList();
	}

	public bool HasCollided() {
		return hasCollided;
	}
}