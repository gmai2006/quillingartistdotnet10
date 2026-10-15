using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiService.Entities;

/// <summary>Auto generated from a schema.</summary>
[Table("products")]
public class Products
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("available")]
    public int Available { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("brand")]
    public string? Brand { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("rating")]
    public double? Rating { get; set; }

    [Column("num_reviews")]
    public int? NumReviews { get; set; }

    [NotMapped]
    public List<OrderItems>? OrderItems { get; set; }
}
