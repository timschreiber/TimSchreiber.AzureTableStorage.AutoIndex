using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimSchreiber.AspNetCoreIdentity.AzureTableStorage.Stores;
using TimSchreiber.AzureTableStorgae.ExampleSite.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.TryAddScoped<IEmailSender, DebugEmailSender>();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>()
    .AddAzureTableStorageStores(options =>
    {
        options.TablePrefix = builder.Configuration.GetValue<string>("Identity:AzureTableStorage:TablePrefix");
        options.ChunkSize = builder.Configuration.GetValue<int>("Identity:AzureTableStorage:ChunkSize");
        options.StorageConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.IndexTableSuffix = builder.Configuration.GetValue<string>("Identity:AzureTableStorage:IndexTableSuffix");
    }).AddDefaultTokenProviders()
    .AddUserManager<UserManager<IdentityUser>>()
    .AddSignInManager<SignInManager<IdentityUser>>()
    .AddRoleManager<RoleManager<IdentityRole>>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
