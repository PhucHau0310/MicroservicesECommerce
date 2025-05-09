using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Repositories.Interfaces
{
    public interface IOrderPlacedProducer
    {
        Task SendMessage<T>(T message);
    }
}
