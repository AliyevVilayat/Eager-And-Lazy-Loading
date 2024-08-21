using Loading.Lazy.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
); ;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    //EntityFrameworkCore default olaraq Eager Loading tətbiq etdiyi üçün yalnız UseLazyLoadingProxies() method tətbiq edilərək Lazy Loading'ə çevirə bilərik
    opt.UseLazyLoadingProxies();
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PC"));

});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
