using System.Reflection;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API - Locadora de Ve\u00edculos",
        Description =
            "API RESTful para gest\u00e3o de uma locadora de ve\u00edculos, desenvolvida em ASP.NET Core 8 " +
            "com Entity Framework Core e SQL Server Express.\n\n" +
            "**Entidades:** Fabricante, Categoria de Ve\u00edculo, Ve\u00edculo, Cliente e Aluguel.\n\n" +
            "**C\u00f3digos de resposta utilizados:**\n" +
            "- `200 OK` - consulta ou opera\u00e7\u00e3o realizada com sucesso\n" +
            "- `201 Created` - registro criado (o cabe\u00e7alho Location aponta para o novo recurso)\n" +
            "- `204 No Content` - altera\u00e7\u00e3o ou exclus\u00e3o conclu\u00edda\n" +
            "- `400 Bad Request` - dados de entrada inv\u00e1lidos\n" +
            "- `404 Not Found` - registro inexistente\n" +
            "- `409 Conflict` - viola\u00e7\u00e3o de regra de neg\u00f3cio (duplicidade ou depend\u00eancia)\n" +
            "- `500 Internal Server Error` - erro n\u00e3o previsto, tratado pelo middleware global"
    });

    var xml = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

    if (File.Exists(xml))
        options.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Locadora de Ve\u00edculos v1");
        options.DocumentTitle = "API - Locadora de Ve\u00edculos";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
