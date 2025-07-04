using System;
using System.Collections.Generic;
using Godot;

public partial class Unit : Node2D {
    [Export]
    PackedScene warriorScene;

    [Export] private int warriorCount = 4;

    List<Vector2> offsets = new() {
      new Vector2(20, 20),
      new Vector2(-20, 20),
      new Vector2(20, -20),
      new Vector2(-20, -20),
    };
    List<Warrior> warriors = new();

    private Random random = new Random();

    public override void _Ready() {
        // Called every time the node is added to the scene.
        // Initialization here.

        // for (var i = 0; i < warriorCount; i++)
        // {
        //   // Add the offset vector to the list
        //   offsets.Add(GetRandomOffsetVariance());
        // }

        for(var i = 0; i < warriorCount; i++) {
          SpawnWarrior(offsets[i]);
        }
    }

    public override void _Process(double delta) {

      for (int i = 0; i < warriors.Count; i++) {
        var warrior = warriors[i];
        warrior.MoveTowardsPosition(Position + offsets[i], delta);
      }
    }

    public void MoveUnit(Vector2 newPosition, float newRotation) {
      Position = newPosition;
      Rotation = newRotation;
      offsets.Clear();

    }

    public Vector2 GetRandomOffsetVariance()
    {
      float x = (float)((random.NextDouble() * 5) - 2.5);
      float y = (float)((random.NextDouble() * 5) - 2.5);

      return new Vector2(x, y);
    }

    public void SpawnWarrior(Vector2 Offset) {
      if (warriorScene == null)
      {
        GD.PrintErr("No Warrior scene assigned!");
        return;
      }

      Warrior newWarrior = warriorScene.Instantiate<Warrior>();

      if (newWarrior == null)
      {
        GD.PrintErr("Warrior null!");
        return;
      }

      Node parentScene = GetParent();

      newWarrior.Position = Position + Offset;
      newWarrior.Rotation = random.Next(0, 360);
      parentScene.CallDeferred("add_child", newWarrior);

      warriors.Add(newWarrior);
    }
}
