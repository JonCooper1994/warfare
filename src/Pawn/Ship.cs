using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
  public float CurrentRotation;
  public Vector2I GridPosition;

  public Sprite2D sprite;
  private int frames;
  private float anglePerFrame;

  public List<Vector2> FullGridPath = new();
  public List<float> CurrentRotationPath = new();

  public FinalGridPath FinalGridPath;
  public float CollisionPoint;

  private float moveTimeElapsed = 0.0f;

  public List<Move> MoveOrders = [];

  public override void _Ready()
  {
    sprite = GetNode<Sprite2D>("Sprite");
    frames = sprite.Hframes;
    anglePerFrame = 360 / frames;
  }

  public void ResetTurn() {
    FullGridPath.Clear();
    CurrentRotationPath.Clear();
    FinalGridPath = new(Direction, new());
    moveTimeElapsed = 0.0f;
  }

  public void ResetRound() {
    MoveOrders.Clear();
  }

  public override void _Process(double delta) {
  }

  public bool AnimateMove(float delta) {
    var t = Mathf.Ease(moveTimeElapsed / 1.5f, -1.5f);
    GD.Print("T:" + t);

    CurrentRotation = Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(CurrentRotationPath[0]), Mathf.DegToRad(CurrentRotationPath[1]), t));

    if (FullGridPath.Count == 3) {
      AnimateTurn(t);
    }
    else {
      AnimateForward(t);
    }

    sprite.Frame = mod(Mathf.RoundToInt(CurrentRotation / anglePerFrame), frames);

    moveTimeElapsed += delta;

    if (moveTimeElapsed >= 1.5f) {
      return true;
    }

    return false;
  }

  public int mod(int x, int m) {
    return ((x % m) + m) % m;
  }

  public void AnimateForward(float elapsedTurnRatio) {
    List<Vector2> intendedMovementPath = new();

    foreach (var gridPos in FullGridPath)
    {
      intendedMovementPath.Add(GridUtils.GridToWorldPos(gridPos));
    }

    var actualFinalPosition = GridUtils.GridToWorldPos(FinalGridPath.path.Last());

    if(elapsedTurnRatio < CollisionPoint) {
      Position = intendedMovementPath[0].Lerp(intendedMovementPath[1], elapsedTurnRatio);

    } else {
      var startPos = intendedMovementPath[0].Lerp(intendedMovementPath[1], CollisionPoint);
      Position = startPos.Lerp(actualFinalPosition, (elapsedTurnRatio - CollisionPoint) / (1.0f - CollisionPoint));
    }
  }

  public void AnimateTurn(float elapsedTurnRatio) {
    List<Vector2> intendedMovementPath = new();

    foreach (var gridPos in FullGridPath)
    {
      intendedMovementPath.Add(GridUtils.GridToWorldPos(gridPos));
    }

    var actualFinalPosition = GridUtils.GridToWorldPos(FinalGridPath.path.Last());

    if(elapsedTurnRatio < CollisionPoint) {
      Position = QuadraticBezier(intendedMovementPath[0], intendedMovementPath[1], intendedMovementPath[2], elapsedTurnRatio);
    } else {
      var startPos = QuadraticBezier(intendedMovementPath[0], intendedMovementPath[1], intendedMovementPath[2], CollisionPoint);
      Position = startPos.Lerp(actualFinalPosition, (elapsedTurnRatio - CollisionPoint) / (1.0f - CollisionPoint));
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
