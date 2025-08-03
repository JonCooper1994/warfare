namespace Warfare.Battle;

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Hand : Node2D{

  [Export] private PackedScene cardWidgetScene;

  private List<Card> drawPile = new();
  private List<Card> currentHand = new();
  private List<Card> discardPile = new();

  private Dictionary<Card, CardWidget> cardWidgets = new();

  private int cardSpacing = 100;

  public void ShuffleDrawPile() {
    drawPile = drawPile.OrderBy(_ => GD.RandRange(0, 9999)).ToList();
  }

  public void DrawCards(int amount) {
    for(var i = 0; i < amount; i++) {
      if (drawPile.Count <= 0) {
        drawPile = discardPile;
        discardPile = [];
        ShuffleDrawPile();
      }

      DrawCard();
    }

    UpdateWidgetPositions();
  }

  private void UpdateWidgetPositions() {
    var totalWidth = cardSpacing * currentHand.Count;
    var halfWidth = totalWidth / 2;

    var x = -halfWidth;
    foreach (var cardWidget in cardWidgets.Values) {
      cardWidget.CallDeferred("MoveTo", new Vector2(x, 0));
      x += cardSpacing;
    }
  }

  private Card DrawCard() {
    if (drawPile.Count <= 0) {
      drawPile = discardPile;
      discardPile = [];
      ShuffleDrawPile();
    }

    var newCard = drawPile.First();
    currentHand.Add(drawPile.First());
    drawPile.Remove(newCard);

    SpawnCardWidget(newCard);

    return newCard;
  }

  public void DiscardHand() {
    foreach (var card in currentHand)
    {
      discardPile.Add(card);
      currentHand.Remove(card);
    }

    currentHand.Clear();
  }

  public void AddDemoCard() {
    var card = new Card("Heal", "Heal 1 to any ally", 1);
    drawPile.Add(card);
  }

  private void SpawnCardWidget(Card newCard) {
    var newCardWidget = cardWidgetScene.Instantiate<CardWidget>();

    newCardWidget.SetCard(newCard);

    cardWidgets.Add(newCard, newCardWidget);

    CallDeferred("add_child", newCardWidget);
  }

  public void HoverCard() {
    UpdateWidgetPositions();
  }
}
