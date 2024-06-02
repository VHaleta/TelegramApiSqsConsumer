using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.ComponentModel;

namespace TelegramApiSqsConsumer
{
    public class SqsMessageConsumer
    {
        private AmazonSQSClient _sqsClient;
        private readonly string _seviceUrl;
        private readonly string _awsregion;
        private readonly string _accessKey;
        private readonly string _secret;
        private bool _isPolling;
        private int _delay;
        private string _queueUrl;
        private int _maxNumberOfMessages;
        private int _messageWaitTimeSeconds;
        private CancellationTokenSource _source;
        private CancellationToken _token;
        [DefaultValue(false)]
        public bool IsOnlyWatching { get; set; }
        [DefaultValue(true)]
        public bool IsFetchingMultipleTimes { get; set; }

        public SqsMessageConsumer()
        {
            _accessKey = "ignore";
            _secret = "ignore";
            _queueUrl = "https://sqs.eu-central-1.localhost.localstack.cloud:4566/000000000000/mainQueue";
            _seviceUrl = "http://localhost:4566";
            _awsregion = "eu-central-1";
            _messageWaitTimeSeconds = 20;
            _maxNumberOfMessages = 10;
            _delay = 1000;

            BasicAWSCredentials basicCredentials = new BasicAWSCredentials(_accessKey, _secret);
            var config = new AmazonSQSConfig
            {
                ServiceURL = _seviceUrl,
                AuthenticationRegion = _awsregion
            };

            _sqsClient = new AmazonSQSClient(basicCredentials, config);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);
        }

        public async Task Listen()
        {
            _isPolling = true;

            int i = 0;
            try
            {
                _source = new CancellationTokenSource();
                _token = _source.Token;

                while (_isPolling)
                {
                    if (!IsFetchingMultipleTimes) _isPolling = false;
                    i++;
                    Console.Write(i + ": ");
                    await FetchFromQueue();
                    Thread.Sleep(_delay);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Application Terminated: " + ex.Message);
            }
            finally
            {
                _source.Dispose();
            }
        }

        private async Task FetchFromQueue()
        {
            var attributesList = new List<string>
            {
                "MESSAGE_CONTEXT",
                "MESSAGE_SENDDATETIME"
            };

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = _maxNumberOfMessages,
                WaitTimeSeconds = _messageWaitTimeSeconds,
                MessageAttributeNames = attributesList
            };
            ReceiveMessageResponse receiveMessageResponse;

            receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, _token);

            if (receiveMessageResponse.Messages.Count != 0)
            {
                for (int i = 0; i < receiveMessageResponse.Messages.Count; i++)
                {
                    string messageBody = receiveMessageResponse.Messages[i].Body;

                    Console.WriteLine("Message Received");

                    var meta = MessageMeta.CreateMetaFromMessage(receiveMessageResponse.Messages[i]);
                    Console.WriteLine(
                        $"Message context:\t{meta.Context}\n" +
                        $"Message date:\t{meta.SendDateTime}\n" +
                        $"Message content:\t{messageBody}\n" +
                        $"------------------------------------------------------");

                    if (!IsOnlyWatching)
                        await DeleteMessageAsync(receiveMessageResponse.Messages[i].ReceiptHandle);
                }
            }
            else
            {
                Console.WriteLine("No Messages to process");
            }
        }

        private async Task DeleteMessageAsync(string recieptHandle)
        {
            DeleteMessageRequest deleteMessageRequest = new DeleteMessageRequest();
            deleteMessageRequest.QueueUrl = _queueUrl;
            deleteMessageRequest.ReceiptHandle = recieptHandle;

            DeleteMessageResponse response = await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
        }

        protected void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            _source.Cancel();
            _isPolling = false;
        }
    }
}