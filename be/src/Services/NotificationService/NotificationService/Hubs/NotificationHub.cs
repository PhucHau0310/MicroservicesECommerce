using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;

namespace NotificationService.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    userId = Context.User?.FindFirst("sub")?.Value;
                }

                _logger.LogInformation($"Attempting connection for user: {userId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    // Support multiple connections per user
                    _userConnections.AddOrUpdate(
                        userId,
                        new HashSet<string> { Context.ConnectionId },
                        (_, connections) =>
                        {
                            connections.Add(Context.ConnectionId);
                            return connections;
                        });

                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");

                    // Send a test message to confirm connection
                    await Clients.Caller.SendAsync("Connected", new
                    {
                        userId = userId,
                        connectionId = Context.ConnectionId,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Authentication failed - No user ID found in token");
                    Context.Abort();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    if (_userConnections.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(Context.ConnectionId);
                        if (connections.Count == 0)
                        {
                            _userConnections.TryRemove(userId, out _);
                        }
                    }

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                    _logger.LogInformation($"User {userId} disconnected from {Context.ConnectionId}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync", ex);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string userId, string message, string type)
        {
            try
            {
                _logger.LogInformation($"Attempting to send notification to user {userId}: {message}");

                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    var notification = new
                    {
                        type = type,
                        message = message,
                        timestamp = DateTime.UtcNow
                    };

                    foreach (var connectionId in connections)
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                    }

                    _logger.LogInformation($"Notification sent to user {userId} on {connections.Count} connection(s)");
                }
                else
                {
                    _logger.LogWarning($"No active connections found for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {userId}");
            }
        }

        // Method to broadcast to all connected clients
        public async Task BroadcastMessage(string message, string type = "broadcast")
        {
            try
            {
                var notification = new
                {
                    type = type,
                    message = message,
                    timestamp = DateTime.UtcNow
                };

                await Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Broadcast message sent: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting message");
                throw;
            }
        }
    }
}
