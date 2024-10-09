var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NanoTemp.UI.Client._Imports).Assembly);
app.UseResponseCompression();

app.MapPost("/measure", async (Measurement m) =>
{
    var ctx = app.Services.GetRequiredService<IHubContext<TempHub>>();
    await ctx.Clients.All.SendAsync("ReceiveMessage", m);
    return new OkResult();
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapHub<TempHub>("/temphub");


app.Run();