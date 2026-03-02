using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GameGroupDbContext>
(
    options => options.UseInMemoryDatabase("TestDB")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameGroupDbContext>();
    db.Database.EnsureCreated();

    if (!db.GameInfos.Any())
    {
        db.GameInfos.AddRange(
            new GameInfo { Id = 1, Name = "Game A" },
            new GameInfo { Id = 2, Name = "Game B" },
            new GameInfo { Id = 3, Name = "Game C" }
        );
        db.SaveChanges();
    }

    if (!db.GameGroupInfos.Any())
    {
        var rnd = new Random();
        var gameIds = db.GameInfos.Select(g => g.Id).ToList();

        var groups = new List<GameGroupInfo>();

        for (int i = 1; i <= 50; i++)
        {
            groups.Add(new GameGroupInfo
            {
                Id = i,
                Title = $"Group {i}",
                Description = $"Description {rnd.Next(1000, 9999)}",
                GameId = gameIds[rnd.Next(gameIds.Count)],
                ImageUrl = (i % 3 == 0)
                    ? "https://media.istockphoto.com/id/1142192548/vector/man-avatar-profile-male-face-silhouette-or-icon-isolated-on-white-background-vector.jpg?s=1024x1024&w=is&k=20&c=ISYAkNv_k8SCN_pHkYWqlWdGSbirhx_yCigo7QC8NAw="
                    : null
            });
        }

        db.GameGroupInfos.AddRange(groups);
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
