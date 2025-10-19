using AuthApi.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Registracija MongoDB odjemalca
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(builder.Configuration.GetValue<string>("MongoDB:ConnectionString")));
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Registracija UserService
builder.Services.AddSingleton<UserService>();

// Konfiguriramo ASP.NET Core aplikacijo
builder.Services.AddControllers();

// Dodajanje CORS pravilnika
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev strežnik
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dodajanje Swagger UI (za lažje testiranje)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI za testiranje API-ja
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware za uporabo kontrolerjev
app.UseHttpsRedirection();

app.UseCors("AllowAngularClient"); // Uporaba CORS pravilnika

app.UseAuthorization();

app.MapControllers();

app.Run();