using System.Text.Json;
using System.Text.Json.Serialization;

namespace Installation.Abstractions.Installation
{
    public static class InstallationRequestFile
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static async Task WriteAsync(string path, InstallationRequest request, CancellationToken cancellationToken)
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, request, Options, cancellationToken);
        }

        public static async Task<InstallationRequest> ReadAsync(string path, CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<InstallationRequest>(stream, Options, cancellationToken)
                ?? throw new InvalidOperationException("Installation request is invalid.");
        }
    }
}
