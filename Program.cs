using WebApplication1.Database;
using WebApplication1.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Ajouter Swagger pour la documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RevoluDbContext>();

// Ajouter le service CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Ajouter les services pour les contrôleurs
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return;
    }
    await next.Invoke();
});

// Activer Swagger uniquement en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Activer le middleware CORS ici

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/auth") || context.Request.Path.StartsWithSegments("/api/user"), builder =>
{
    builder.UseMiddleware<AuthorizationMiddleware>();
});

app.UseHttpsRedirection();
app.UseAuthorization();

// Activer les contrôleurs
app.MapControllers();

app.Run();