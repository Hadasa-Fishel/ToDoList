using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi;

[Table("tasks")]
public partial class Item
{
    [Column("Id")]
    public int Id { get; set; }

    [Column("Title")]
    public string? Name { get; set; }

    [Column("IsCompleted")]
    public bool? IsComplete { get; set; }
}
