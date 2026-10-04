using Microsoft.EntityFrameworkCore;
using Tricount.Data;
using Tricount.Repositories;
using Tricount.Repositories.Interfaces;
using Tricount.Services;
using Tricount.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register EF Core InMemory Database
builder.Services.AddDbContext<TricountDbContext>(options =>
    options.UseInMemoryDatabase("TricountDb"));

// Register application layers
builder.Services.AddScoped<ITricountRepository, TricountRepository>();
builder.Services.AddScoped<ITricountService, TricountService>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
