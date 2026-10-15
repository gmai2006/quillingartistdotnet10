using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiService.Entities;

/// <summary>Auto generated from a schema.</summary>
[Table("orders")]
public class Orders
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("shipping_address_id")]
    public int? ShippingAddressId { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("total")]
    public decimal Total { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [NotMapped]
    public List<OrderItems>? OrderItems { get; set; }
}
