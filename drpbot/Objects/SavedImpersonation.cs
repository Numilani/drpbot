using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace drpbot.Objects;

[Table("SavedImpersonations")]
[Index(nameof(name), IsUnique = true)]
public class SavedImpersonation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string name { get; set; }
    public string imgurl { get; set; }
}