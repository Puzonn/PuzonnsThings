

namespace PuzonnsThings.Models.Yahtzee;

public class YahtzeePlayer
{
    private const int MaxDices = 5;

    public int Points { get; private set; } = 0;

    public string ConnectionId { get; set; } = string.Empty;
    public string PlayerName { get; set; }
    public int UserId { get; set; }

    public int RollCount { get; set; } = 2;
    public bool CanRoll => RollCount > 0;

    public uint LobbyId { get; set; }

    public readonly YahtzeeDice[] Dices = new YahtzeeDice[MaxDices];
    public readonly List<YahtzeeSettedPoint> SettedPoints = new List<YahtzeeSettedPoint>();

    public YahtzeePlayer(string playerName, string connectionId, int userId)
    {
        PlayerName = playerName;
        ConnectionId = connectionId;
        UserId = userId;

        for (int i = 0; i < MaxDices; i++)
        {
            Dices[i] = new YahtzeeDice(i);
        }
    }

    public void Restart(uint lobbyId)
    {
        LobbyId = lobbyId;
        Points = 0;
        SettedPoints.Clear();
    }

    public bool HasMoves()
    {
        if(SettedPoints.Count == 13)
        {
            return false;
        }

        return true;
    }

    public (bool success, int points) SetPointsFromPoint(YahtzeePointType point)
    {
        if (SettedPoints.Any(x => x.Point == point))
        {
            return (false, 0);
        }

        int points = 0;

        switch (point)
        {
            case YahtzeePointType.One:
                points = Dices.Where(x => x.RolledDots == 1).Count() * 1;
                break;
            case YahtzeePointType.Two:
                points = Dices.Where(x => x.RolledDots == 2).Count() * 2;
                break;
            case YahtzeePointType.Three:
                points = Dices.Where(x => x.RolledDots == 3).Count() * 3;
                break;
            case YahtzeePointType.Four:
                points = Dices.Where(x => x.RolledDots == 4).Count() * 4;
                break;
            case YahtzeePointType.Five:
                points = Dices.Where(x => x.RolledDots == 5).Count() * 5;
                break;
            case YahtzeePointType.Six:
                points = Dices.Where(x => x.RolledDots == 6).Count() * 6;
                break;
            case YahtzeePointType.Chance:
                {
                    points = Dices.Sum(x => x.RolledDots);
                    break;
                }
            case YahtzeePointType.ThreeOfaKind:
                {
                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach (YahtzeeDice dice in Dices)
                    {
                        if (counts.ContainsKey(dice.RolledDots))
                        {
                            counts[dice.RolledDots]++;
                        }
                        else
                        {
                            counts[dice.RolledDots] = 1;
                        }
                    }
                    if (counts.Any(count => count.Value >= 3))
                    {
                        points = Dices.Sum(x => x.RolledDots);
                    }

                    break;
                }
            case YahtzeePointType.FourOfaKind:
                {
                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach (YahtzeeDice dice in Dices)
                    {
                        if (counts.ContainsKey(dice.RolledDots))
                        {
                            counts[dice.RolledDots]++;
                        }
                        else
                        {
                            counts[dice.RolledDots] = 1;
                        }
                    }

                    if(counts.Values.Any(count => count >= 4))
                    {
                        points = Dices.Sum(x=> x.RolledDots);
                    }

                    break;
                }
            case YahtzeePointType.SmallStraight:
                {
                    int[] sortedDices = Dices.Select(x=>x.RolledDots).OrderBy(x => x).ToArray();
                    int count = 0;

                    for (int i = 0; i < sortedDices.Length - 1; i++)
                    {
                        if (sortedDices[i + 1] - sortedDices[i] == 1)
                        {
                            count++;
                            if (count == 3)
                            {
                                points = 30;
                                break;
                            }
                        }
                        else if (sortedDices[i + 1] - sortedDices[i] != 0)
                        {
                            count = 0;
                        }
                    }
                    break;
                }
            case YahtzeePointType.LargeStraight:
                {
                    int[] sortedDices = Dices.Select(x => x.RolledDots).OrderBy(x => x).ToArray();
                    if(sortedDices.SequenceEqual(new[] { 1, 2, 3, 4, 5 }) ||
                      sortedDices.SequenceEqual(new[] { 2, 3, 4, 5, 6 }))
                    {
                        points = 40;
                    }
                    break;
                }
            case YahtzeePointType.Yahtzee:
                {
                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach (YahtzeeDice dice in Dices)
                    {
                        if (counts.ContainsKey(dice.RolledDots))
                        {
                            counts[dice.RolledDots]++;
                        }
                        else
                        {
                            counts[dice.RolledDots] = 1;
                        }
                    }
                    if (counts.Count == 1)
                    {
                        points = 50;
                    }
                    break;
                }
            case YahtzeePointType.FullHouse:
                {
                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach (YahtzeeDice dice in Dices)
                    {
                        if (counts.ContainsKey(dice.RolledDots))
                        {
                            counts[dice.RolledDots]++;
                        }
                        else
                        {
                            counts[dice.RolledDots] = 1;
                        }
                    }
                    int[] frequencies = counts.Values.ToArray();

                    if (frequencies.Contains(2) && frequencies.Contains(3))
                    {
                        points = 25;
                    }
                    break;
                }
        }

        Points += points;

        SettedPoints.Add(new YahtzeeSettedPoint()
        {
            Point = point,
            PointsFromPoint = points,
        });

        return (true, points);
    }
}