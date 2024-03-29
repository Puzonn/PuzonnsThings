﻿using Backend.Models.Interfaces;
using PuzonnsThings.Models.Yahtzee;

namespace PuzonnsThings.Hubs.Yahtzee;

public class YahtzeePlayer : IPlayer
{
    private const int MaxDices = 5;

    public int Points { get; private set; } = 0;
    public int UserId { get; set; }
    public int GameTime { get; set; }   

    /// <summary>
    /// Each player can roll max two times their dices
    /// </summary>
    public int RollCount { get; set; } = 2;

    public uint ConnectedLobbyId { get; set; }

    public bool CanPlay { get; set; } = true;

    public string ConnectionId { get; set; }
    public string Username { get; }
    public string Avatar { get; }

    public bool CanRoll => RollCount > 0;
    public bool IsReady { get; set; } = true;

    public readonly YahtzeeDice[] Dices = new YahtzeeDice[MaxDices];
    public readonly List<YahtzeePlacedPoint> PlacedPoints = new List<YahtzeePlacedPoint>();

    public YahtzeePlayer(string playerName, string avatar, string connectionId, int userId)
    {
        Avatar = avatar;
        Username = playerName;
        ConnectionId = connectionId;
        UserId = userId;

        for (int i = 0; i < MaxDices; i++)
        {
            Dices[i] = new YahtzeeDice(i);
        }
    }

    public void Restart(uint lobbyId)
    {
        ConnectedLobbyId = lobbyId;
        Points = 0;
        PlacedPoints.Clear();
    }

    /// <summary>
    /// Checks if player have any moves, there is max 13 moves
    /// </summary>
    public bool HasMoves() => PlacedPoints.Count != 13;

    public (bool success, int points) SetPointsFromPoint(YahtzeePointType point)
    {
        if (PlacedPoints.Any(x => x.Point == point))
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

                    if (counts.Values.Any(count => count >= 4))
                    {
                        points = Dices.Sum(x => x.RolledDots);
                    }

                    break;
                }
            case YahtzeePointType.SmallStraight:
                {
                    int[] sortedDices = Dices.Select(x => x.RolledDots).OrderBy(x => x).ToArray();
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
                    if (sortedDices.SequenceEqual(new[] { 1, 2, 3, 4, 5 }) ||
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

        PlacedPoints.Add(new YahtzeePlacedPoint()
        {
            Point = point,
            PointsFromPoint = points,
        });

        return (true, points);
    }
}