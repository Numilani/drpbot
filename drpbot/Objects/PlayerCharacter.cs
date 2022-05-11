using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace drpbot.Objects;

[Table("PlayerCharacters")]
public class PlayerCharacter: ICharacter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public ulong DiscordUserId { get; set; }
    
    public string Name { get; set; }
    public string Race { get; set; }
    public string Description { get; set; } = "";
    
    [NotMapped]
    public IEnumerable<IObtainable> Inventory { get; set; }
    [NotMapped]
    public IDictionary<string, object?> Equipment { get; set; }
    
    [Column(TypeName = "integer")]
    public CharacterStatus Status { get; set; }

    public string StatusNotes { get; set; } = "";
}