namespace drpbot.Objects;

public interface ICharacter
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public IEnumerable<IObtainable> Inventory { get; set; }
    
    public IDictionary<string, object?> Equipment { get; set; }
}