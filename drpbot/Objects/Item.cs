using System.ComponentModel.DataAnnotations;

namespace drpbot.Objects;

public class Item
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    
    public string DisplayedItemType { get; set; }
    public string DisplayedItemSecondaryType { get; set; }

    public double Weight { get; set; } = 0.0;

    // CustomFlags is a list of flags used for checking behaviors (as a replacement for having distinct classes of objects)
    public Dictionary<string, string> CustomFlags { get; set; } = new();

    // InteractionMatrix is an optional, serialized dynamic class used for handling custom behavior (not implemented)
    public Dictionary<string, string> InteractionMatrix { get; set; } = new();

}