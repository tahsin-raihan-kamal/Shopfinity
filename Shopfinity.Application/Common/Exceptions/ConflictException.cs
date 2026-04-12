namespace Shopfinity.Application.Common.Exceptions;

/// <summary>Maps to HTTP 409 Conflict (e.g. duplicate wishlist entry).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
