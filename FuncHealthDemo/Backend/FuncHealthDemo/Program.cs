using FirebaseAdmin;
using FuncHealthDemo.DB;
using FuncHealthDemo.Filters;
using FuncHealthDemo.Middleware;
using FuncHealthDemo.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Initialize Firebase Admin SDK
if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        // Option 1: Try environment variable first
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.GetApplicationDefault(),
        });
        Console.WriteLine("✅ Firebase Admin SDK initialized from environment variable");
    }
    catch
    {
        try
        {
            // Option 2: Try loading from file in project root
            var serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "serviceAccountKey.json");
            if (File.Exists(serviceAccountPath))
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath),
                });
                Console.WriteLine("✅ Firebase Admin SDK initialized from serviceAccountKey.json");
            }
            else
            {
                Console.WriteLine("⚠️  Firebase Admin SDK not initialized.");
                Console.WriteLine("   Set GOOGLE_APPLICATION_CREDENTIALS or place serviceAccountKey.json in project root");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Firebase initialization error: {ex.Message}");
        }
    }
}

// Configure SQLite Database
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=health.db"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();

// Register Filters
builder.Services.AddScoped<ValidateUserIdFilter>();

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS before authentication and routing
app.UseCors("ReactFrontend");

// Use Authentication Middleware
Console.WriteLine("✅ Using FIREBASE AUTH MODE - Token verification required");
app.UseFirebaseAuth();

app.MapControllers();

app.Run();
