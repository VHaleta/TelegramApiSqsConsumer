using Amazon.SQS.Model;

namespace TelegramApiSqsConsumer
{
    public static class MessageTypeConstants
    {
        public const string MessageContext = "MESSAGE_CONTEXT";
        public const string MessageCreatedDateTimeOffsetKey = "MESSAGE_SENDDATETIME";
    }

    public class MessageMeta
    {
        public Guid MessageId { get; init; }
        public DateTimeOffset SendDateTime { get; init; }
        public string Context {  get; init; }

        public static MessageMeta InitHeader(string context, DateTimeOffset sendDate)
        {
            return new MessageMeta
            {
                MessageId = new Guid(),
                SendDateTime = sendDate,
                Context = context,
            };
        }

        public static MessageMeta CreateMetaFromMessage(Message message)
        {
            return new MessageMeta
            {
                MessageId = Guid.Parse(message.MessageId),
                Context = message.MessageAttributes[MessageTypeConstants.MessageContext].StringValue,
                SendDateTime = DateTimeOffset.Parse(message.MessageAttributes[MessageTypeConstants.MessageCreatedDateTimeOffsetKey].StringValue)
            };
        }
    }
}
