using System.Text;

namespace TelegramApiSqsConsumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                SqsMessageConsumer sqsConsumer = new SqsMessageConsumer()
                {
                    IsOnlyWatching = false,
                    IsFetchingMultipleTimes = true,
                };
                await sqsConsumer.Listen();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
