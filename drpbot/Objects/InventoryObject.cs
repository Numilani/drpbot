using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace drpbot.Objects;

[Table("InventoryItems")]
public class InventoryObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public ulong OwnerId { get; set; }
    
    [Column(TypeName = "integer")]
    public OwnerType OwnerType { get; set; }
    
    /// <summary>
    /// Foreign key of underlying item. Use InventoryManager.GetUnderlyingItem() instead of referencing this directly. 
    /// </summary>
    public Item UnderlyingItem { get; set; }

    public string? CustomName { get; set; }
    
    public string? CustomDescription { get; set; }
    public string? AdditionalDescription { get; set; }

    public Dictionary<string, string> ItemState { get; set; } = new();

    public string Name => CustomName ?? UnderlyingItem.Name;
    public string Description => CustomDescription ?? UnderlyingItem.Description;
}