using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

// --- פרטי החיבור (כאן נמצא החשוד המיידי) ---
// וודאי שוב שזו הסיסמה הנכונה מ-Clever Cloud!
var connectionString = "Server=bohp6s0zhjhusturosug-mysql.services.clever-cloud.com;Port=3306;Database=bohp6s0zhjhusturosug;Uid=u0eqxgbmbsqxqkjf;Pwd=t9gk2N2s7rRwGcoald5B;";

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 2))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// --- בדיקה 1: האם הקוד התעדכן? ---
app.MapGet("/", () => "Todo API is running (Version 5 DEBUG MODE)");

// --- בדיקה 2: דף "מלשין" שיגלה לנו מה השרת רואה ---
app.MapGet("/debug", () => 
{
    return Results.Ok(new { 
        Message = "Check your connection string carefully",
        CurrentConnectionString = connectionString 
    });
});

app.MapGet("/items", async (ToDoDbContext db) =>
{
    try
    {
        return Results.Ok(await db.Items.AsNoTracking().ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem($"DB Error: {ex.Message}");
    }
});

// שאר ה-Endpoints...
app.MapPost("/items", async (Item item, ToDoDbContext db) => { db.Items.Add(item); await db.SaveChangesAsync(); return Results.Created($"/items/{item.Id}", item); });
app.MapPut("/items/{id:int}", async (int id, Item input, ToDoDbContext db) => { var item = await db.Items.FindAsync(id); if (item is null) return Results.NotFound(); item.Name = input.Name; item.IsComplete = input.IsComplete; await db.SaveChangesAsync(); return Results.NoContent(); });
app.MapDelete("/items/{id:int}", async (int id, ToDoDbContext db) => { var item = await db.Items.FindAsync(id); if (item is null) return Results.NotFound(); db.Items.Remove(item); await db.SaveChangesAsync(); return Results.NoContent(); });

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    try { db.Database.Migrate(); Console.WriteLine("Migration Success!"); }
    catch (Exception ex) { Console.WriteLine($"Migration Failed: {ex.Message}"); }
}

app.Run();