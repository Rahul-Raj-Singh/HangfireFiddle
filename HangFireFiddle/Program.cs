using Hangfire;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<DummyJob>();

builder.Services.AddHangfire(cfg => cfg
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlDbConnection")));

// Hangfire server is responsible for executing the job
// client and server can be different apps
builder.Services.AddHangfireServer(cfg => 
    cfg.SchedulePollingInterval = TimeSpan.FromSeconds(1));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Endpoints
app.MapGet("/schedule", ([FromQuery] bool fail, IBackgroundJobClient client) =>
{
    client.Schedule<DummyJob>(job => job.ExecuteAsync(fail), TimeSpan.Zero);
    return Results.Ok("Job scheduled!");
});

app.MapGet("/schedule-recurring", ([FromQuery] bool fail, IRecurringJobManager client) =>
{
    client.AddOrUpdate<DummyRecurringJob>(
        "CleanupJob", 
        x => x.ExecuteAsync(fail), 
        "0/30 * * * * *");
    
    return Results.Ok("Job scheduled!");
});

app.MapGet("/jobDetails/{jobId}", (string jobId) =>
{
    var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
    var latest = jobDetails.History.OrderByDescending(x => x.CreatedAt).First();
    
    return Results.Ok(new
    {
        JobId = jobId,
        latest.StateName,
        latest.Reason,
        latest.CreatedAt
    });
});

app.UseHangfireDashboard();

app.Run();

// Jobs
public class DummyJob(ILogger<DummyJob> logger)
{
    public async Task ExecuteAsync(bool fail)
    {
        logger.LogInformation("Starting dummy job.."); 
        
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        if (fail) throw new ApplicationException("Job failed!");
        
        logger.LogInformation("Completed dummy job."); 
    }
}

[DisableConcurrentExecution(0)]
[AutomaticRetry(Attempts = 0)]
public class DummyRecurringJob(ILogger<DummyRecurringJob> logger)
{
    public async Task ExecuteAsync(bool fail)
    {
        logger.LogInformation("Starting recurring job.."); 
        
        await Task.Delay(TimeSpan.FromSeconds(20));
        
        if (fail) throw new ApplicationException("Job failed!");
        
        logger.LogInformation("Completed recurring job."); 
    }
}