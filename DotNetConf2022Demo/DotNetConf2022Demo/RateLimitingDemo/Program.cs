using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(_ =>
{
    _.AddFixedWindowLimiter("NotSoFast", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});

//builder.Services.AddRateLimiter(_ =>
//{
//    _.AddPolicy("NotSoFast", httpContext =>
//    {
//        return RateLimitPartition.GetFixedWindowLimiter(httpContext.Connection.Id,
//            key => new FixedWindowRateLimiterOptions()
//            {
//                PermitLimit = 4,
//                Window = TimeSpan.FromSeconds(12),
//                QueueLimit = 2
//            });
//    });
//});

//builder.Services.AddRateLimiter(_ =>
//{
//    _.AddPolicy("User", httpContext =>
//    {
//        if (httpContext.Request.Headers.ContainsKey("apiKey"))
//        {
//            // Todo: Validate the key
//            return RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers["apiKey"].ToString(),
//                key => new FixedWindowRateLimiterOptions()
//                {
//                    PermitLimit = 1000,
//                    Window = TimeSpan.FromSeconds(60),
//                    QueueLimit = 100
//                });
//        }

//        return RateLimitPartition.GetFixedWindowLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
//            key => new FixedWindowRateLimiterOptions()
//            {
//                PermitLimit = 10,
//                Window = TimeSpan.FromSeconds(60),
//                QueueLimit = 2
//            });
//    });
//});

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MapControllers();

app.MapControllers().RequireRateLimiting("NotSoFast");

app.Run();
