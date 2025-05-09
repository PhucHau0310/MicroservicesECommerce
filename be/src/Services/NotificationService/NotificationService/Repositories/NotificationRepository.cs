using Microsoft.AspNetCore.SignalR;
using NotificationService.Infrastructure.Hubs;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using NotificationService.Data;
using NotificationService.Helper;

namespace NotificationService.Infrastructure.Persistence
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MongoDbContext _mongoDb;
        private readonly RedisHelper _redis;
        private readonly NotificationHub _notifcaitionHub;

        public NotificationRepository(MongoDbContext mongoDb, RedisHelper redis, NotificationHub notifcaitionHub)
        {
            _mongoDb = mongoDb;
            _redis = redis;
            _notifcaitionHub = notifcaitionHub;
        }

        public async Task SendNotificationAsync(Guid userId, string message, string type)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required");
            }

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type
            };

            await _mongoDb.Notifications.InsertOneAsync(notification);
            await _redis.AddNotificationAsync(userId, message);
            await _notifcaitionHub.SendNotification(userId.ToString(), message, type);  
        }

        public async Task<List<NotificationRedis>> GetUnreadNotificationsAsync(Guid userId)
        {
            return await _redis.GetUnreadNotificationsAsync(userId);
        }
            
        public async Task MarkAsReadAsync(Guid userId, string notificationId)
        {
            try
            {
                var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
                await _mongoDb.Notifications.UpdateOneAsync(
                    n => n.Id == notificationId && n.UserId == userId,
                    update);

                // Clear Redis cache cho user này
                await _redis.ClearNotificationsAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error marking notification {notificationId} as read for user {userId}");
            }
        }
    }
}
