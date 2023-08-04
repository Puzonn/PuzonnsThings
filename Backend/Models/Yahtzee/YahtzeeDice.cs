namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public sealed class YahtzeeDice
{
    public int Index { get; }
    public int RolledDots { get; set; } = 0;

    public YahtzeeDice(int index)
    {
        Index = index;

        Roll();
    }

    public void Roll()
    {
        RolledDots = Random.Shared.Next(1, 7);
    }

    public void Roll(Random random)
    {
        RolledDots = random.Next(1, 7);
    }
}