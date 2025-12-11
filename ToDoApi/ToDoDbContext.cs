using Microsoft.EntityFrameworkCore;

namespace ToDoApi;

public class ToDoDbContext : DbContext
{
    // בנאי (Constructor) שמקבל את אפשרויות החיבור מ-Program.cs
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
        : base(options)
    {
    }

    // ייצוג הטבלה במסד הנתונים
    public DbSet<Item> Items => Set<Item>();
}