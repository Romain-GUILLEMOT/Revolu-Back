using WebApplication1.Database;
using WebApplication1.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Ajouter Swagger pour la documentation meme si cela est innutile.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RevoluDbContext>();

// Ajouter le service CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Remplace par l'origine autorisée
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Nécessaire si tu utilises des cookies ou des tokens
    });
});
// Ajouter les services pour les contrôleurs
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowFrontend");
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

app.UseHttpsRedirection(); // WTF is that
app.UseAuthorization();

// Activer les contrôleurs
app.MapControllers();

app.Run();