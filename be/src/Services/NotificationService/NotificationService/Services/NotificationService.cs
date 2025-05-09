using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NotificationRedis>> GetUnreadNotificationsAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadNotificationsAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid userId, string notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(userId, notificationId);
        }

        public async Task SendNotificationAsync(Guid userId, string message, string type)
        {
            await _notificationRepository.SendNotificationAsync(userId, message, type); 
        }
    }
}
