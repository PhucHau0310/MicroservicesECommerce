using NotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string message, string type);
        Task<List<NotificationRedis>> GetUnreadNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid userId, string notificationId);
    }
}
