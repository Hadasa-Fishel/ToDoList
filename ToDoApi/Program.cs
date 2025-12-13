using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

// --- פרטי החיבור המדויקים מתוך צילום המסך שלך ---
var connectionString = "Server=bc4j7gx7f3zjqrv2hhgr-mysql.services.clever-cloud.com;Port=3306;Database=bc4j7gx7f3zjqrv2hhgr;Uid=u3dokk46ypo2nirc;Pwd=yAQ4aW5EnL67sFKmQHGJ;";

// הדפסה ללוג כדי שנהיה בטוחים שהסיסמה התעדכנה
Console.WriteLine($"DEBUG: Connection String in use: {connectionString}");

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        connectionString,
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

app.MapGet("/", () => "Todo API is running (Password Updated)");

app.MapGet("/items", async (ToDoDbContext db) =>
{
    try
    {
        return Results.Ok(await db.Items.AsNoTracking().ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem("DB Error: " + ex.Message);
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

// יצירת טבלאות (עם הגנה מקריסה)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Success! Connected to DB.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

app.Run();