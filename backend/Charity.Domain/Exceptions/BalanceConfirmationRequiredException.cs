namespace Charity.Domain.Exceptions;

public sealed class BalanceConfirmationRequiredException(string message) : Exception(message);
