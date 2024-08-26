using Microsoft.AspNetCore.Mvc;
//using VodenkoWeb.Model;
using VodenkoWeb.Services;
using DataAccess.Entities;

namespace VodenkoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("latest")]
        public async Task<ActionResult<List<Notification>>> GetLatestNotifications()
        {
            var notifications = await _notificationService.GetLatestNotifications(5);
            return notifications;
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetNewNotificationCount()
        {
            var count = await _notificationService.GetNewNotificationCount();
            return count;
        }

        [HttpPost("markAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsRead();
            return Ok();
        }

        [HttpPost("markAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsRead(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNotification([FromBody] Notification notification)
        {
            if (notification == null || string.IsNullOrWhiteSpace(notification.Message))
            {
                return BadRequest(new { error = "Invalid notification" });
            }

            await _notificationService.AddNotification(notification);
            return Ok();
        }

        [HttpGet("paged")]
        public async Task<ActionResult<List<Notification>>> GetPagedNotifications(int pageNumber, int pageSize)
        {
            var result = await _notificationService.GetPagedNotifications(pageNumber, pageSize);
            return result;
        }

    }
}
