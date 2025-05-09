using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Domain.Entities
{
    public class NotificationRedis
    {
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
