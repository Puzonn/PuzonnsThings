namespace PuzonnsThings.Models.Yahtzee;

[Serializable]
public class YahtzeeRoomModel
{
    public int Id { get; set; }
    public string Creator { get; set; }
    public int CreatorId { get; set; }

    public YahtzeeRoomModel(string creator, int creatorId)
    {
        Creator = creator;
        CreatorId = creatorId;
    }
}
