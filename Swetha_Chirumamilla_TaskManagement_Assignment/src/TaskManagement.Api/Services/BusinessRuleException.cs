namespace TaskManagement.Api.Services;

public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
