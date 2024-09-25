
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ntl.AuthSS.OrderManagement.Business
{
    public class MetaDataQueueService
    {
        private readonly QueueClient _cloudQueue;
        public MetaDataQueueService(string storageConnectionString, string metaDataQueue)
        {
            QueueServiceClient account = new QueueServiceClient(storageConnectionString);
            _cloudQueue = account.GetQueueClient(metaDataQueue);
        }
        public async Task AddMsgToQueue(MetaDataQueueModel metaData)
        {
            var messageAsJson = JsonConvert.SerializeObject(metaData);
            await _cloudQueue.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageAsJson)));
        }
    }
}
