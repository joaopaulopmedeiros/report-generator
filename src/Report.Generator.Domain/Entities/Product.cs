namespace Report.Generator.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public double Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
