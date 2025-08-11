using Godot;
using System;
using System.Net;
using Warfare.Battle;

public partial class Projectile : Node2D {

  [Export] private float timePerTile = 0.5f;

  public Vector2I StartGridPos { get; set; }
  public Vector2 StartWorldPos { get; set; }
  public Vector2I TargetGridPos { get; set; }
  public Vector2 TargetWorldPos { get; set; }

  public int GridDistance;

  public float elapsedTime;

  public float height = 5.0f;

  public Sprite2D ProjectileSprite;

  public override void _Ready() {
    ProjectileSprite = GetNode<Sprite2D>("ProjectileSprite");
  }

  public void Init(Vector2I startGridPos, Vector2I targetGridPos) {
    StartGridPos = startGridPos;
    TargetGridPos = targetGridPos;

    StartWorldPos = GridUtils.GridToWorldPos(StartGridPos);
    TargetWorldPos = GridUtils.GridToWorldPos(TargetGridPos);

    GridDistance = Mathf.RoundToInt((StartGridPos - TargetGridPos).Length());

  }

  public bool Animate(float delta) {
    Position = StartWorldPos.Lerp(TargetWorldPos, elapsedTime / (GridDistance * timePerTile));

    ProjectileSprite.Position = new Vector2(0.0f, -height);

    elapsedTime += delta;
    height -= delta * 1.5f;

    if (elapsedTime >= GridDistance * timePerTile) {
      QueueFree();
      return true;
    }

    return false;
  }




}
