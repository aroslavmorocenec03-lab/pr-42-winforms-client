using System.Text.Json.Serialization;
using PostService.Dtos;
using PostService.Infrastructure;
using PostService.Mappings;
using PostService.BusinessLogic;
using PostService.DataAccess;
using Microsoft.Data.Sqlite;
using Microsoft.OpenApi;
using PostService;
using PostService.CommonTypes;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://localhost:5211");
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// SwaggerGen і HTTP JSON мають однаково показувати enum як рядки.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PostService — облік поштових відправлень",
        Version = "v1",
        Description = "REST API для отримання, створення, редагування та видалення поштових відправлень. "
            + "Грошові суми вказуються у гривнях, вага — у кілограмах, габарити — у сантиметрах. "
            + "У JSON десятковий роздільник — крапка."
    });
    foreach (var assembly in new[] { typeof(PostingEndpoints).Assembly, typeof(PostingGetDto).Assembly, typeof(DeliveryType).Assembly })
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml"));
    options.UseAllOfToExtendReferenceSchemas();
    options.SupportNonNullableReferenceTypes();
});
builder.Services.AddScoped<IPostingService, PostingService>();
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "PostingDb.db");
var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
builder.Services.AddScoped<IPostingRepository>(_ => new PostingRepository(connectionString));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var repository = (PostingRepository)scope.ServiceProvider.GetRequiredService<IPostingRepository>();
    repository.CreateDb();
}
app.UseMiddleware<BusinessExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "PostService v1");
    options.DocumentTitle = "Документація PostService";
    options.RoutePrefix = "swagger";
    options.DisplayRequestDuration();
});

app.MapGet("/", PostingEndpoints.Status)
    .WithName("GetServiceStatus").WithTags("Стан сервісу")
    .Produces<string>(StatusCodes.Status200OK, "text/plain");

app.MapGet("/postings", PostingEndpoints.GetAll)
    .WithName("GetPostings").WithTags("Поштові відправлення")
    .Produces<PostingGetDto[]>(StatusCodes.Status200OK);

app.MapGet("/postings/{id:int}", PostingEndpoints.GetById)
    .WithName("GetPostingById").WithTags("Поштові відправлення")
    .Produces<PostingGetDto>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);

app.MapPost("/postings", PostingEndpoints.Create)
    .WithName("CreatePosting").WithTags("Поштові відправлення")
    .Accepts<PostingPostDto>("application/json")
    .Produces<PostingGetDto>(StatusCodes.Status201Created).ProducesValidationProblem();

app.MapPut("/postings/{id:int}", PostingEndpoints.Update)
    .WithName("UpdatePosting").WithTags("Поштові відправлення")
    .Accepts<PostingPutDto>("application/json")
    .Produces<PostingGetDto>(StatusCodes.Status200OK).ProducesValidationProblem()
    .Produces(StatusCodes.Status404NotFound);

app.MapDelete("/postings/{id:int}", PostingEndpoints.Delete)
    .WithName("DeletePosting").WithTags("Поштові відправлення")
    .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);

app.Run();
