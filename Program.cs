using ImageResizer.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
//  SERVICE CONFIGURATION
//  Registers MVC controllers and the ImageResizeService
//  used for processing image uploads.
// ==========================================================
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ImageResizeService>(); // Shared service instance

var app = builder.Build();

// ==========================================================
//  EXCEPTION HANDLING & SECURITY
//  Uses exception pages in development; HSTS in production.
// ==========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ==========================================================
//  MIDDLEWARE PIPELINE
//  Configures HTTPS redirection, routing, and authorization.
// ==========================================================
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// ==========================================================
//  ROUTE MAPPING
//  Maps MVC controllers and enables static file access
//  for generated images and CSS/JS assets.
// ==========================================================
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Image}/{action=Index}/{id?}")
    .WithStaticAssets();

// ==========================================================
//  APPLICATION ENTRY POINT
// ==========================================================
app.Run();
