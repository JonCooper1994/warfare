using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ShipAI
{

    Ship myShip;

    Battle battle;

    private AIData aiData;

    public ShipAI(Ship ship, Battle battle, AIData aiData) {
        myShip = ship;
        this.battle = battle;
        this.aiData = aiData;
    }

    public List<Order> GenerateOrders() {
      return ApproachAndShoot();
    }

    List<Order> ApproachAndShoot() {
        List<Order> orders = new();

        var targetShip = getClosestEnemyShip(myShip.GridPosition);

        var moves = Pathfinder.AStar(battle, myShip.GridPosition, myShip.Direction, targetShip.GridPosition);

		    for (int i = moves.Count; i < 4; i++) {
			    moves.Add(Move.Skip);
		    }

        for (int i = 0; i < 4; i++) {
            Order newOrder = new(){
                Move = moves[i]
            };

            orders.Add(newOrder);
        }

        orders = AddShootOrders(orders);

        return orders;
    }

    // TODO: obey limits to the number of times AI can shoot
    List<Order> AddShootOrders(List<Order> orders) {
        var pos = myShip.GridPosition;
        var dir = myShip.Direction;

        var orderIndex = 0;
        foreach (var order in orders) {
            var move = order.Move;

            pos = battle.PredictedPosition(move, dir, pos);
            dir = battle.PredictedDirection(move, dir, pos);

            List<LocalDirection> weaponDirections = [LocalDirection.Left, LocalDirection.Right];

            foreach(var direction in weaponDirections) {
                var vector = dir.AddLocalDirection(direction).Vector();

                var sum = 0.0;

                for (int i = 0; i < 4; i++) {
                    var target = pos + (vector * i);

                    var probability = aiData.GetPlayerShipProbability(orderIndex, target);

                    sum += 1.0f - ((1.0f - sum) * (1.0f - probability));

                    if(probability > aiData.playerShipPercentiles[orderIndex][50]) {
                        order.cannonsFired[direction] = true;
                    }
                }

                if(sum > aiData.playerShipPercentiles[orderIndex][70]) {
                    order.cannonsFired[direction] = true;
                }
            }

            orderIndex++;
        }

        return orders;
    }

    float DistanceBetween(Vector2I a, Vector2I b) {
        return Math.Abs((a - b).Length());
    }

    Ship getClosestEnemyShip(Vector2I pos) {
        var enemyShips = battle.ships.Where(s => s.faction != myShip.faction).ToList();

        if(enemyShips.Count == 0) {
            return null;
        }

        Ship closestShip = enemyShips.First();
        float closestDistance = DistanceBetween(closestShip.GridPosition, pos);

        foreach (var ship in enemyShips) {
            if(ship == myShip) {continue;}

            var distance = DistanceBetween(ship.GridPosition, pos);
            if(distance < closestDistance) {
                closestDistance = distance;
                closestShip = ship;
            }
        }

        return closestShip;
    }

}
