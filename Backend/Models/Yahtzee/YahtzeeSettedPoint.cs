﻿namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public class YahtzeeSettedPoint
{
    public YahtzeePointType Point { get; set; }
    public int PointsFromPoint { get; set; }
}