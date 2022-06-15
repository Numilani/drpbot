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
    
    public int BaseHealth { get; set; } = 100;
    public int BaseVitality { get; set; } = 100;
    public int BaseAttack { get; set; } = 1;
    public int BaseDefense { get; set; } = 1;
    public int BaseSpeed { get; set; } = 10;

    public int CurrentHealth { get; set; } = 100;
    public int CurrentVitality { get; set; } = 100;

    public List<EquipmentSlot> UsableEquipmentSlots { get; set; } = new List<EquipmentSlot>()
    {
        EquipmentSlot.HEAD,
        EquipmentSlot.FACE,
        EquipmentSlot.LEFT_EAR,
        EquipmentSlot.RIGHT_EAR,
        EquipmentSlot.NECKLACE,
        EquipmentSlot.LEFT_HAND_WEARABLE,
        EquipmentSlot.RIGHT_HAND_WEARABLE,
        EquipmentSlot.TORSO,
        EquipmentSlot.LEFT_ARM,
        EquipmentSlot.RIGHT_ARM
    };

    [Column(TypeName = "integer")]
    public CharacterStatus Status { get; set; }

    public string StatusNotes { get; set; } = "";
}