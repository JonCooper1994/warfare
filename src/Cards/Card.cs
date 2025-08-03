namespace Warfare.Battle;

public class Card {
  public string Name;
  public string Description;
  public int ManaCost;

  public Card(string Name, string Description, int ManaCost) {
    this.Name = Name;
    this.Description = Description;
    this.ManaCost = ManaCost;
  }
}
