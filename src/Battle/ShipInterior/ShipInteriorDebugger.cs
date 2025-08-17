using System.Collections.Generic;
using Godot;
using Warfare.Battle;

public static class ShipInteriorDebugger
{
    static PackedScene debugDot;

    static List<Node2D> nodes = new();

    public static void Init() {
        debugDot = GD.Load<PackedScene>("res://src/Battle/DebugDot.tscn");
    }

    public static void CreateNodeOnGrid(ShipInterior shipInterior, Vector2I gridPos, float scale = 1.0f) {
        var node = debugDot.Instantiate<Node2D>();
        nodes.Add(node);
        shipInterior.AddChild(node);

        node.Scale = new Vector2(scale, scale);

        var g2w = shipInterior.GridToWorld(gridPos);
		    node.Position = new Vector2(g2w.X, g2w.Y);
    }

    public static void Clear() {
        foreach (var node in nodes) {
            node.QueueFree();
        }

        nodes = new();
    }
}
