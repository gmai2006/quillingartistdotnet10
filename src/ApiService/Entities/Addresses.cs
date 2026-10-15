using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiService.Entities;

/// <summary>Auto generated from a schema.</summary>
[Table("addresses")]
public class Addresses
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("line1")]
    public string Line1 { get; set; } = string.Empty;

    [Column("line2")]
    public string? Line2 { get; set; }

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("state")]
    public string? State { get; set; }

    [Column("postal_code")]
    public string PostalCode { get; set; } = string.Empty;

    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [NotMapped]
    public List<Orders>? Orders { get; set; }
}
