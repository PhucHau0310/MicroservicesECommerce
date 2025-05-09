using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Messaging.Interfaces
{
    public interface IPaymentProducer
    {
        Task SendMessage<T>(T message);
    }
}
