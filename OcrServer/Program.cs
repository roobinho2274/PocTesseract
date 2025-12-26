using OcrCore.Services.Implmentation;
using OcrCore.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<IOcrPageService, OcrPageService>();
builder.Services.AddSingleton<IExtractTextService, ExtractTextService>();
builder.Services.AddSingleton<IOcrExtractor, OcrExtractor>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OCR API v1");
    c.RoutePrefix = "swagger"; // http://host:port/swagger
});
app.UseAuthorization();

app.MapControllers();

app.Run();
