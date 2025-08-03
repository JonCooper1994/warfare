using Godot;
using System;
using Warfare.Battle;

public partial class CardWidget : Node2D {
  private Card card;

  private RichTextLabel nameText;
  private RichTextLabel descriptionText;

  private Tween tween;


  public override void _Ready() {
    nameText = GetNode<RichTextLabel>("nameText");
    descriptionText = GetNode<RichTextLabel>("descriptionText");
    UpdateWidget();
  }

  public void MoveTo(Vector2 newPosition) {
    if (tween != null) {
      tween.Kill();
    }

    tween = GetTree().CreateTween();

    // Interpolate to target position at set speed
    tween.TweenProperty(this, "position", newPosition, 1.0f)
      .SetTrans(Tween.TransitionType.Linear)
      .SetEase(Tween.EaseType.InOut);

  }

  public void SetCard(Card newCard) {
    card = newCard;
    UpdateWidget();
  }

  public void UpdateWidget() {
    if (nameText != null && card != null) {
      nameText.SetText(card.Name);
      descriptionText.SetText(card.Description);
    }
  }
}
