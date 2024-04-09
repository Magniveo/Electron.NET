namespace ElectronNET.API.Entities;

/// <summary>
/// </summary>
public class Blob : IPostData
{
    /// <summary>
    ///     The UUID of the Blob being uploaded
    /// </summary>
    public string BlobUUID { get; set; }

    /// <summary>
    ///     The object represents a Blob
    /// </summary>
    public string Type { get; } = "blob";
}