using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

public class AIData
{
    // How likely is it that a grid position will have a player ship in it on a particular turn
    // TODO: make it less likely that a player will take a path that damages them
    Dictionary<int, Dictionary<Vector2I, float>> playerShipProbabilities;

    Dictionary<Ship, Dictionary<int, Dictionary<Vector2I, float>>> allShipProbabilities = new();

    public float maxEnemyShipProbability {get; private set;} = 0.0f;

    public Dictionary<int, Dictionary<int, float>> playerShipPercentiles {get; private set;} = new();

    Battle battle;

    public AIData(Battle battle) {
        this.battle = battle;
        playerShipProbabilities = NewProbabilityMap();
        CalculateProbabilities();
        CalculatePlayerShipPercentiles();
    }

    public float GetPlayerShipProbability(int orderIndex, Vector2I pos) {
        if(!playerShipProbabilities[orderIndex].ContainsKey(pos)) {
            return 0.0f;
        }

        return playerShipProbabilities[orderIndex][pos];
    }

    public void CalculateProbabilities() {
        foreach(var ship in battle.ships) {
          allShipProbabilities[ship] = CalculateProbabilityForShip(ship);
        }

        foreach (var ship in battle.ships){
          if (ship.IsPlayer) {
            AddProbability(playerShipProbabilities, allShipProbabilities[ship]);
          }
        }

        maxEnemyShipProbability = GetMax(playerShipProbabilities);

        //DebugProbabilityMap(playerShipProbabilities);
    }

    public Dictionary<int, Dictionary<Vector2I, float>> CalculateProbabilityForShip(Ship ship) {
        var availableMoves = ship.AvailableMoves();

        // Every possible permutation of movements
        List<List<Move>> moveLists = CalculateMoveLists(availableMoves);

        Dictionary<int, Dictionary<Vector2I, float>> p = NewProbabilityMap();

        foreach (var moveList in moveLists) {
            Vector2I pos = ship.GridPosition;
            Direction dir = ship.Direction;

            var orderIndex = 0;

            var probabilitySum = 1.0f;

            foreach (var move in moveList) {
                pos = battle.PredictedPosition(move, dir, pos);
                dir = battle.PredictedDirection(move, dir, pos);
                var collisions = battle.PredictedCollisionCount(move, dir, pos);

                if (collisions > 0) {
                  probabilitySum *= (0.1f / collisions);
                }

                p[orderIndex][pos] = p[orderIndex][pos] + probabilitySum;

                orderIndex++;
            }
        }

        var maxMoveCount = Convert.ToInt32(Math.Pow(4, availableMoves.Count));

       // p = DivideBy(p, maxMoveCount);
        p = Normalise(p);

        //DebugProbabilityMap(p);

        return p;
    }

    public void CalculatePlayerShipPercentiles() {
        foreach(var item in playerShipProbabilities) {
            var index = item.Key;
            List<float> sortedProbabilities = new();

            foreach (var pair in item.Value) {
                if(pair.Value > 0) {
                    sortedProbabilities.Add(pair.Value);
                }
            }

            sortedProbabilities.Sort();

            playerShipPercentiles[index] = new();

            var total = sortedProbabilities.Count();

            for (int i = 0; i < 100; i++) {
                var percentileIndex = Convert.ToInt32(Math.Floor(i * (total/100.0f)));
                playerShipPercentiles[index][i] = sortedProbabilities[percentileIndex];
            }
        }
    }

    private float GetMax(Dictionary<int, Dictionary<Vector2I, float>> p) {
        var max = 0.0f;

        foreach (var item in p) {
            var index = item.Key;
            foreach (var pair in item.Value) {
                if(pair.Value > max) {
                    max = pair.Value;
                }
            }
        }

        return max;
    }

    private Dictionary<int, Dictionary<Vector2I, float>> DivideBy(Dictionary<int, Dictionary<Vector2I, float>> p, int divisor) {
        foreach (var item in p) {
            var index = item.Key;
            foreach (var pair in item.Value) {
                p[index][pair.Key] = p[index][pair.Key] / divisor;
            }
        }

        return p;
    }

    private Dictionary<int, Dictionary<Vector2I, float>> Normalise(Dictionary<int, Dictionary<Vector2I, float>> p) {
      foreach (var item in p) {
        float maxValue =  item.Value.Values.Max();
        var index = item.Key;
        foreach (var pair in item.Value) {
          p[index][pair.Key] = p[index][pair.Key] / maxValue;
        }
      }

      return p;
    }

    // Adds probability by doing 1 - (P notA) * (P notB)
    private Dictionary<int, Dictionary<Vector2I, float>> AddProbability(Dictionary<int, Dictionary<Vector2I, float>> a, Dictionary<int, Dictionary<Vector2I, float>> b) {
        foreach(var item in a) {
            var index = item.Key;
            foreach (var pair in item.Value) {

                a[index][pair.Key] = 1.0f - ((1.0f - a[index][pair.Key]) * (1.0f - b[index][pair.Key]));
            }
        }

        return a;
    }

    public void DebugProbabilityMap(Dictionary<int, Dictionary<Vector2I, float>> p) {
        foreach (var item in p[3]){
            var pos = item.Key;
            var probability = item.Value;
            if(probability > 0.0f) {
                Debugger.CreateNodeOnGrid(battle, pos, probability);
            }
        }
    }

    public void DebugPlayerShipProbabilities(int orderIndex) {
      foreach (var item in playerShipProbabilities[orderIndex]){
        var pos = item.Key;
        var probability = item.Value;
        if(probability > 0.0f) {
          Debugger.CreateNodeOnGrid(battle, pos, probability);
        }
      }
    }

    public Dictionary<int, Dictionary<Vector2I, float>> NewProbabilityMap() {
        Dictionary<int, Dictionary<Vector2I, float>> p = new() {
            {0, new()},
            {1, new()},
            {2, new()},
            {3, new()},
        };

        for(int i = 0; i < 4; i++) {
            for (int x = 0; x < battle.Size.X; x++) {
                for (int y = 0; y < battle.Size.Y; y++) {
                    p[i][new Vector2I(x, y)] = 0;
                }
            }
        }

        return p;
    }


    // Returns all possible movement paths based on the list of moves provided
    public List<List<Move>> CalculateMoveLists(List<Move> moves) {
        List<List<Move>> allMoves = new();

        for (int i1 = 0; i1 < moves.Count; i1++){
            for (int i2 = 0; i2 < moves.Count; i2++){
                for (int i3 = 0; i3 < moves.Count; i3++){
                    for (int i4 = 0; i4 < moves.Count; i4++){
                        allMoves.Add(
                            new(){
                                moves[i1],
                                moves[i2],
                                moves[i3],
                                moves[i4]
                            }
                        );
                    }
                }
            }
        }

        return allMoves;
    }

}
