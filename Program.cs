using System.Collections.Immutable;
using System.Reflection;
using Elastic.Apm.NetCoreAll;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
Configuration(env,configuration);
builder.Host.UseSerilog();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAllElasticApm(builder.Configuration);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void Configuration(string env,IConfigurationRoot configuration)
{
    

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigurationELS(configuration,env))
        .CreateLogger();

}

ElasticsearchSinkOptions ConfigurationELS(IConfigurationRoot configuration,string env)
{
    var t = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-" +
            $"{env.ToLower().Replace(".", "-")}-{DateTime.Now:yyyy-MM}";
    return new ElasticsearchSinkOptions(new Uri(configuration["ELKConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-" +
                      $"{env.ToLower().Replace(".","-")}-{DateTime.Now:yyyy-MM}"
    };
}