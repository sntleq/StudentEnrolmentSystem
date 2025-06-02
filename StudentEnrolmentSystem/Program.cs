using StudentEnrolmentSystem.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddScoped<TimeMachineController>();
builder.Services.AddScoped<StudentApiController>();
builder.Services.AddScoped<CourseApiController>();
builder.Services.AddScoped<ProgramApiController>();
builder.Services.AddScoped<CurriculumApiController>();
builder.Services.AddScoped<FacultyApiController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=TimeMachine}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();