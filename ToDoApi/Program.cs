using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
        return Results.Problem(ex.ToString());
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
    if (item is null)
    {
        return Results.NotFound();
    }

    item.Name = input.Name;
    item.IsComplete = input.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id:int}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ToDoDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // אם יש שגיאה בחיבור או ב-Migration, רצוי לכתוב אותה ל-Log
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}app.Run();
