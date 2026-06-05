var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.Run();