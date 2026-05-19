using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQL Server + Swagger
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// ENDPOINTS: CRUD de Facturas para autónomos España
app.MapGet("/facturas", async (AppDbContext db) =>
    await db.Facturas.OrderByDescending(f => f.Fecha).ToListAsync());

app.MapPost("/facturas", async (Factura factura, AppDbContext db) =>
{
    factura.Fecha = DateTime.Now;
    factura.Numero = $"2026-{await db.Facturas.CountAsync() + 1:D4}";
    factura.IVA = 21; // IVA España
    db.Facturas.Add(factura);
    await db.SaveChangesAsync();
    return Results.Created($"/facturas/{factura.Id}", factura);
});

app.MapDelete("/facturas/{id}", async (int id, AppDbContext db) =>
{
    var f = await db.Facturas.FindAsync(id);
    if (f is null) return Results.NotFound();
    db.Facturas.Remove(f);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// MODELOS: Esto es tu tabla SQL
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }
    public DbSet<Factura> Facturas => Set<Factura>();
}

public class Factura
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = "";
    public string Concepto { get; set; } = "";
    public decimal BaseImponible { get; set; }
    public decimal IVA { get; set; }
    public decimal Total => BaseImponible * (1 + IVA / 100);
}
