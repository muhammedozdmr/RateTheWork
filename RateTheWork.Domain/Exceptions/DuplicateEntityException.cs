namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Duplicate entity exception'ı
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string DuplicateField { get; }
    public string DuplicateValue { get; }

    public DuplicateEntityException(string entityName, string duplicateField, string duplicateValue)
        : base($"{entityName} with {duplicateField} '{duplicateValue}' already exists.")
    {
        EntityName = entityName;
        DuplicateField = duplicateField;
        DuplicateValue = duplicateValue;
    }
}

