using System;
using Godot;

public partial class Warrior : Node2D {

  [Export] private float speed;
  public override void _Ready() {
      // Called every time the node is added to the scene.
      // Initialization here.
  }

  public override void _Process(double delta) {
      // Called every frame. Delta is time since the last frame.
      // Update game logic here.
  }


  public void MoveTowardsPosition(Vector2 targetPosition, double deltaTime) {
    float floatDelta = (float)deltaTime;

    Vector2 direction = (targetPosition - Position).Normalized();

    Position += direction * speed * floatDelta;
  }
}
