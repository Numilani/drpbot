namespace drpbot.Objects;

public interface ICharacter
{
    public string Name { get; set; }
    
    public string Description { get; set; }

    public int BaseHealth { get; set; }
    public int BaseAttack { get; set; }
    public int BaseDefense { get; set; }
    public int BaseSpeed { get; set; }
}