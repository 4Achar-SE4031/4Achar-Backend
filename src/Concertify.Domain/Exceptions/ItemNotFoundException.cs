using System.Runtime.Serialization;


namespace Concertify.Domain.Exceptions;

[Serializable]
public class ItemNotFoundException : Exception
{
    public readonly int? ItemId;
    public ItemNotFoundException(int itemId)
    {
        ItemId = itemId;
    }

    public ItemNotFoundException(string? message) : base(message)
    {
    }

    public ItemNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}