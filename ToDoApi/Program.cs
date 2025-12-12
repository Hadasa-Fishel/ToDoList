using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

// --- הכתובת הקבועה (Hardcoded) ---
// שימי לב: הסרתי רווחים, הוספתי פורט, וסידרתי את הפורמט שיהיה 100% תקין
var connectionString = "Server=bc4j7gx7f3zjqrv2hhgr-mysql.services.clever-cloud.com;Port=3306;Database=bc4j7gx7f3zjqrv2hhgr;User=u3dokk46ypo2nirc;Password=yAQ4aW5EnL67sFKmQHGJ;";

// --- דיבוג: הדפסה ללוג כדי לוודא שהכתובת קיימת ---
Console.WriteLine("--------------------------------------------------");
Console.WriteLine($"DEBUG: Connection String Length: {connectionString.Length}");
Console.WriteLine($"DEBUG: Target Database: bc4j7gx7f3zjqrv2hhgr");
Console.WriteLine("--------------------------------------------------");

// הגדרת ה-DB עם הגרסה הקבועה למניעת קריסות
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

app.MapGet("/", () => "Todo API is running (Version 3.0 Hardcoded)");

app.MapGet("/items", async (ToDoDbContext db) =>
{
    try
    {
        return Results.Ok(await db.Items.AsNoTracking().ToListAsync());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"CRITICAL ERROR in GET /items: {ex.Message}");
        return Results.Problem(ex.Message);
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

// --- יצירת טבלאות עם מנגנון הגנה מקריסה ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    try
    {
        Console.WriteLine("Attempting to connect to database for migration...");
        db.Database.Migrate();
        Console.WriteLine("Migration Successful!");
    }
    catch (Exception ex)
    {
        // תופס את השגיאה ולא נותן לאתר לקרוס (מונע שגיאה 139)
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"MIGRATION FAILED: {ex.Message}");
        Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
        Console.WriteLine("--------------------------------------------------");
    }
}

app.Run();