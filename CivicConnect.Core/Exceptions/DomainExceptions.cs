namespace CivicConnect.Core.Exceptions;

// Thrown when a business rule is broken (blank title, illegal status change, ...)
public class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string message) : base(message) { }
}

// Thrown when something requested by id does not exist in the database
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, object entityKey)
        : base($"{entityName} with identifier '{entityKey}' was not found.") { }
}
