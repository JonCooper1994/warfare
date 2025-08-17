using System.Collections.Generic;
using Godot;
using Warfare.Battle;

public static class Debugger
{
    static PackedScene debugTile;

    static List<Node2D> nodes = new();

    public static void Init() {
        debugTile = GD.Load<PackedScene>("res://src/Battle/DebugTile.tscn");
    }

    public static void CreateNodeOnGrid(Battle battle, Vector2I gridPos, float scale = 1.0f) {
        var node = debugTile.Instantiate<Node2D>();
        nodes.Add(node);
		    battle.AddChild(node);

        node.Scale = new Vector2(scale, scale);

        var g2w = BattleGridUtils.GridToWorldPos(gridPos);
		    node.Position = new Vector2(g2w.X, g2w.Y);
    }

    public static void Clear() {
        foreach (var node in nodes) {
            node.QueueFree();
        }

        nodes = new();
    }
}
