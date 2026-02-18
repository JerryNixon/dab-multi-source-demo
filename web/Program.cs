var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("dab", client =>
{
    var dabUrl = builder.Configuration["DAB_URL"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(dabUrl);
});

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
