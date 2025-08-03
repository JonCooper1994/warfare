using Godot;
using System;

public enum TileUIState
{
  Normal,
  Hovered,
  SelectedPawn,
  Blocked,
  PossiblePath,
  HighlightedPath,
  PlayerPawn,
  EnemyPawn
}

public partial class Tile : Node2D
{

  private TileUIState _uiState = TileUIState.Normal;

  public override void _Ready() {
    GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(1, 1, 1, 0.0f);
    SetState(TileUIState.Normal);
  }

  public void SetState(TileUIState newUiState) {

    if (newUiState != _uiState) {
      _uiState = newUiState;

      switch (_uiState)
      {
        case TileUIState.Hovered:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(1, 1, 1, 1.0f);
          break;
        case TileUIState.Normal:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(1, 1, 1, 0.0f);
          break;
        case TileUIState.Blocked:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(0, 0, 0, 0.0f);
          break;
        case TileUIState.PossiblePath:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(0, 0, 0.8f, 1.0f);
          break;
        case TileUIState.HighlightedPath:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(0, 0.7f, 0, 1.0f);
          break;
        case TileUIState.PlayerPawn:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(0, 1.0f, 0, 1.0f);
          break;
        case TileUIState.EnemyPawn:
          GetNode<Sprite2D>("SelectionMarker").Modulate = new Color(0.7f, 0.1f, 0.7f, 1.0f);
          break;
      }
    }
  }
}
