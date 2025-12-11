using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

// --- התיקון הסופי: קריאה חכמה של מחרוזת החיבור ---
// 1. קודם מנסים לקחת מהמשתנה הישיר של Render (פותר את הבעיה של שם מסד ריק)
var connectionString = Environment.GetEnvironmentVariable("RENDER_DB_CONNECTION");

// 2. אם זה ריק (כלומר אנחנו במחשב בבית), לוקחים מהקובץ הרגיל
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("ToDoDB");
}
// ---------------------------------------------------

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        connectionString,
        // הגדרה ידנית של הגרסה (פותר את הבעיה של הקריסה 139)
        new MySqlServerVersion(new Version(8, 0, 2))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapGet("/", () => "Todo API is running");

app.MapGet("/items", async (ToDoDbContext db) =>
{
    try
    {
        return Results.Ok(await db.Items.AsNoTracking().ToListAsync());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error in GET /items: " + ex);
        return Results.Problem("An error occurred while fetching items.");
    }
});

app.MapGet("/items/{id:int}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/items", async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id:int}", async (int id, Item input, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = input.Name;
    item.IsComplete = input.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id:int}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// יצירת טבלאות אוטומטית בענן
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.Migrate();
}

app.Run();