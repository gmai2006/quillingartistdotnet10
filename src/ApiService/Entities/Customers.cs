using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiService.Entities;

/// <summary>Auto generated from a schema.</summary>
[Table("customers")]
public class Customers
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [NotMapped]
    public List<Addresses>? Addresses { get; set; }

    [NotMapped]
    public List<Orders>? Orders { get; set; }
}
