
using Microsoft.AspNetCore.Authentication.Cookies;
using Project.Infrastructure.Interfaces;
using Project.Application.Services;
using Project.Repositories;
using Project.Domain;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Project.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuração do MongoDB
builder.Services.Configure<ConfigMongoDb>(builder.Configuration.GetSection("ConfigMongoDb"));

// Registra o cliente MongoDB como Transient
builder.Services.AddTransient<IMongoClient>(sp =>   
{
    var settings = sp.GetRequiredService<IOptions<ConfigMongoDb>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Registrar os serviços necessários

//Usuario -- Cadastro
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();

// Login
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ILoginRepository, LoginRepository>();

// Endereco
builder.Services.AddTransient<IEnderecoService, EnderecoService>();
builder.Services.AddTransient<IEnderecoRepository, EnderecoRepository>();

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Logar";
    });

// Adicionando suporte a sessões
builder.Services.AddDistributedMemoryCache(); // Usar cache de memória para sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Define o tempo de inatividade
    options.Cookie.HttpOnly = true; // Garante que o cookie de sessão seja acessível apenas via HTTP
    options.Cookie.IsEssential = true; // Marca o cookie como essencial para a aplicação
});

var app = builder.Build();

//app.Urls.Add("http://0.0.0.0:5121");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usando o middleware de sessão
app.UseSession(); // Isso habilita o uso de sessões


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();