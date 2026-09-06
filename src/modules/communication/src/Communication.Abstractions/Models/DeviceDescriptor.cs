namespace Communication.Abstractions.Models
{
    public sealed record DeviceDescriptor(
        string Id,
        string Name,
        string? Manufacturer = null,
        string? Model = null
    );
}
