namespace Communication.Abstractions.Entities
{

    public sealed record EntityDescriptor(
        string Key,
        string Name,
        EntityKind Kind,
        string? UnitOfMeasurement = null
    );
}
