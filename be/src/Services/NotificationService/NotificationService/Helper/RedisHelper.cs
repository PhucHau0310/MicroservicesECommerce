using Microsoft.Extensions.Configuration;
using NotificationService.Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotificationService.Helper
{
    public class RedisHelper
    {
        private readonly IDatabase _db;

        public RedisHelper(IConfiguration configuration)
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
            _db = redis.GetDatabase();
        }

        public async Task AddNotificationAsync(Guid userId, string message)
        {
            var notification = new NotificationRedis
            {
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            string key = $"user:{userId}:notifications";
            string jsonValue = JsonSerializer.Serialize(notification);
            await _db.ListLeftPushAsync(key, jsonValue);
        }

        public async Task<List<NotificationRedis>> GetUnreadNotificationsAsync(Guid userId)
        {
            string key = $"user:{userId}:notifications";
            var notifications = await _db.ListRangeAsync(key);

            return notifications.Select(n => JsonSerializer.Deserialize<NotificationRedis>(n)).ToList();
            //return (await _db.ListRangeAsync(key)).ToStringArray();
        }

        public async Task ClearNotificationsAsync(Guid userId)
        {
            string key = $"user:{userId}:notifications";
            await _db.KeyDeleteAsync(key);
        }
    }
}
