using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notification request)
        {
            await _notificationService.SendNotificationAsync(request.UserId, request.Message, request.Type);
            return Ok(new { message = "Notification sent successfully" });
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications(Guid userId)
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPost("read")]
        public async Task<IActionResult> ReadNotifications(Guid userId, string notificationId)
        {
            await _notificationService.MarkAsReadAsync(userId, notificationId);
            return Ok("Mark read notification successfully");
        }
    }
}
