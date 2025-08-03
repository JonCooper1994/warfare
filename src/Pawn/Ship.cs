using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warfare.Battle;

public enum Faction
{
  Player,
  Enemy,
}

public partial class Ship : Node2D {
  [Export] public Faction Faction;

  private int currentHP;

  public int MoveDistanceRemaining { get; private set;}
  public bool IsMoving;

  public Direction Direction = Direction.North;
  private float currentRotation;
  public Vector2I GridPosition;

  public Sprite2D sprite;
  private int frames;
  private float anglePerFrame;

  private List<Vector2> currentMovePath = new();
  private List<float> currentRotationPath = new();

  private float moveTimeElapsed = 0.0f;

  public override void _Ready()
  {
    sprite = GetNode<Sprite2D>("Sprite");
    frames = sprite.Hframes;
    anglePerFrame = 360 / frames;
  }

  public override void _Process(double delta) {
    if (IsMoving) {
      moveTimeElapsed += 0.0f;
      AnimateMove((float)delta);
    }
  }


  public void ExecuteMove(Move move) {
    if (IsMoving) {return;}

    var initialDirection = Direction;
    var initialPosition = GridPosition;

    GridPosition = move.positionChange(initialDirection, initialPosition);
    Direction = move.directionChange(Direction);

    var gridPath = move.pathTaken(initialDirection, initialPosition);

    currentMovePath.Clear();
    foreach (var pos in gridPath) {
     currentMovePath.Add(GridUtils.GridToWorldPos(pos));
    }

    currentRotationPath = [initialDirection.RotationDegrees(), Direction.RotationDegrees()];

    moveTimeElapsed = 0.0f;

    IsMoving = true;
  }

  public void AnimateMove(float delta) {
    moveTimeElapsed += delta;
    var t = moveTimeElapsed / 1.0f;

    currentRotation = Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(currentRotationPath[0]), Mathf.DegToRad(currentRotationPath[1]), t));

    if (currentMovePath.Count == 3) {
      Position = QuadraticBezier(currentMovePath[0], currentMovePath[1], currentMovePath[2], t);
    }
    else {
      Position = currentMovePath[0].Lerp(currentMovePath[1], t);
    }

    GD.Print(currentRotation);
    GD.Print(sprite.Frame);

    sprite.Frame = Mathf.RoundToInt(currentRotation / anglePerFrame) % frames;

    if (moveTimeElapsed >= 1.0f) {
      IsMoving = false;
    }
  }

  private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
  {
    Vector2 q0 = p0.Lerp(p1, t);
    Vector2 q1 = p1.Lerp(p2, t);

    return q0.Lerp(q1, t);
  }

  private float Lerp(float firstFloat, float secondFloat, float by)
  {
    return firstFloat * (1 - by) + secondFloat * by;
  }

}
