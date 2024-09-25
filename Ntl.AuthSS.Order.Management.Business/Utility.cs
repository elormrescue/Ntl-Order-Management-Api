using Azure.Storage.Queues;
using Newtonsoft.Json;
using Ntl.AuthSS.OrderManagement.Business.FileDtos;
using Ntl.AuthSS.OrderManagement.Data.Entities;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public static class Utility
    {

        public static string GenerateOrderNumber(this OrderEntityType orderEntityType)
        {
            var ticks = DateTime.Now.Ticks;
            switch (orderEntityType)
            {
                case OrderEntityType.Manufacturer:
                    return $"MFO{ticks}";
                case OrderEntityType.Tpsaf:
                    return $"TPO{ticks}";
                case OrderEntityType.Ntl:
                    return $"PRO{ticks}";
                default:
                    return $"ORD{ticks}";
            }
        }
        public static string GenerateReturnOrderNumber()
        {
            return $"RET{DateTime.Now.Ticks}";
        }

      

        public static async Task SendToQueue(FileDetailsDto fileDetailsDto, string connectionString,string queueName)
        {

            QueueServiceClient account = new QueueServiceClient(connectionString);
            QueueClient queue = account.GetQueueClient(queueName);
            var messageAsJson = JsonConvert.SerializeObject(fileDetailsDto);
            await queue.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageAsJson)));
        }





    }
}
