namespace Web.Validators;

public sealed class ValidationOptions
{
    public int MinPasswordLength { get; init; }
    public int MaxPasswordLength { get; init; }
    public int MinMuscleTitleLength { get; init; }
    public int MaxMuscleTitleLength { get; init; }
    public int MaxSetRepeats { get; init; }
    public int MaxSetWeight { get; init; }

    public void Validate()
    {
        if (MinPasswordLength <= 0 || MaxPasswordLength < MinPasswordLength)
            throw new InvalidOperationException("Validation password lengths must satisfy 0 < MinPasswordLength <= MaxPasswordLength.");

        if (MinMuscleTitleLength <= 0 || MaxMuscleTitleLength < MinMuscleTitleLength || MaxMuscleTitleLength > 256)
            throw new InvalidOperationException("Validation muscle title lengths must satisfy 0 < MinMuscleTitleLength <= MaxMuscleTitleLength <= 256.");

        if (MaxSetRepeats is <= 0 or > short.MaxValue)
            throw new InvalidOperationException("Validation:MaxSetRepeats must be between 1 and 32767.");

        if (MaxSetWeight is <= 0 or > 999)
            throw new InvalidOperationException("Validation:MaxSetWeight must be between 1 and 999.");
    }
}
