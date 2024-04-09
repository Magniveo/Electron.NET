namespace ElectronNET.API.Entities;

/// <summary>
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// </summary>
    public UpdateInfo UpdateInfo { get; set; } = new();

    /// <summary>
    /// </summary>
    public string[] Download { get; set; }

    /// <summary>
    /// </summary>
    public UpdateCancellationToken CancellationToken { get; set; }
}