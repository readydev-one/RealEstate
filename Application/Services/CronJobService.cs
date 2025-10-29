// RealEstate.Application/Services/CronJobService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Events;

namespace RealEstate.Application.Services;

public class CronJobService : BackgroundService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CronJobService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public CronJobService(
        ITaskRepository taskRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IEventBus eventBus,
        ILogger<CronJobService> logger)
    {
        _taskRepository = taskRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDailyNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cron job execution");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessDailyNotifications()
    {
        _logger.LogInformation("Running daily notification job at {Time}", DateTime.UtcNow);

        // Process task reminders (due in 3 days)
        await ProcessTaskReminders();

        // Process closing date reminders (7 days before)
        await ProcessClosingDateReminders();

        _logger.LogInformation("Daily notification job completed");
    }

    private async Task ProcessTaskReminders()
    {
        var upcomingTasks = await _taskRepository.GetUpcomingTasksAsync(3);

        foreach (var task in upcomingTasks)
        {
            var user = await _userRepository.GetByIdAsync(task.AssignedTo);
            if (user != null)
            {
                await _emailService.SendTaskReminderAsync(
                    user.Email,
                    task.Title,
                    task.DueDate);

                await _eventBus.PublishAsync(new TaskDueSoonEvent
                {
                    TaskId = task.Id,
                    PropertyId = task.PropertyId,
                    AssignedTo = task.AssignedTo,
                    DueDate = task.DueDate,
                    Title = task.Title
                });

                _logger.LogInformation(
                    "Sent task reminder for task {TaskId} to user {UserId}",
                    task.Id, user.Id);
            }
        }
    }

    private async Task ProcessClosingDateReminders()
    {
        var allProperties = await _propertyRepository.GetAllAsync();
        var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);

        var upcomingClosings = allProperties.Where(p =>
            p.ClosingDate.Date == sevenDaysFromNow.Date).ToList();

        foreach (var property in upcomingClosings)
        {
            var notifyUserIds = new List<string> { property.CloserId, property.SellerId };
            notifyUserIds.AddRange(property.BuyerIds);

            foreach (var userId in notifyUserIds.Distinct())
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    await _emailService.SendClosingDateReminderAsync(
                        user.Email,
                        property.Address,
                        property.ClosingDate);

                    _logger.LogInformation(
                        "Sent closing date reminder for property {PropertyId} to user {UserId}",
                        property.Id, user.Id);
                }
            }

            await _eventBus.PublishAsync(new ClosingDateApproachingEvent
            {
                PropertyId = property.Id,
                ClosingDate = property.ClosingDate,
                NotifyUserIds = notifyUserIds.Distinct().ToList()
            });
        }
    }
}