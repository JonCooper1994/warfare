namespace Warfare.Battle;

using System.Collections.Generic;
using Godot;

public partial class LineDrawer : Node2D
{
  public Color LineColor = new Color(1, 0, 0); // Red Line
  public float LineWidth = 0.5f;

  private List<Vector2> Starts = new();
  private List<Vector2> Ends = new();

  public override void _Ready()
  {
    SetZIndex(1000);
  }

  public override void _Draw()
  {
    GD.Print($"DRAW: {Starts.Count}");

    for (int i = 0; i < Starts.Count; i++) {
      var start = Starts[i];
      var end = Ends[i];
      DrawLine(start, end, LineColor, LineWidth);
    }
  }

  public void AddLine(Vector2 start, Vector2 end)
  {
    Starts.Add(start);
    Ends.Add(end);
    QueueRedraw();
  }

  public void Reset() {
    Starts = [];
    Ends = [];
  }
}
