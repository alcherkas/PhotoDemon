using Microsoft.Maui.Controls;
using System.Net.Http;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon asynchronous download control - migrated from VB6 pdDownload.ctl
///
/// Features:
/// - Asynchronous/background file downloads
/// - Multiple concurrent downloads
/// - Download queue management
/// - Success/failure status tracking
/// - Optional checksum verification
/// - Event notification on completion
/// - Invisible at runtime (no UI)
///
/// This control facilitates silent background downloads of update files and other
/// resources without blocking the UI thread. It manages a queue of downloads and
/// raises events when individual or all downloads complete.
///
/// Original VB6: Controls/pdDownload.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdDownload : ContentView
{
    #region Enums

    public enum DownloadStatus
    {
        NotYetStarted = 0,
        Downloading = 1,
        DownloadComplete = 2,
        FailureCallerCanceledDownload = 3,
        FailureButWillTryAgainSoon = 4,
        FailureNotTryingAgain = 5,
        FailureChecksumMismatch = 6
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when a single download completes (successfully or not)
    /// </summary>
    public event EventHandler<DownloadCompletedEventArgs>? FinishedOneItem;

    /// <summary>
    /// Raised when all queued downloads have completed
    /// </summary>
    public event EventHandler<AllDownloadsCompletedEventArgs>? FinishedAllItems;

    #endregion

    #region Event Args

    public class DownloadCompletedEventArgs : EventArgs
    {
        public bool Success { get; }
        public string Key { get; }
        public long OptionalType { get; }
        public byte[]? Data { get; }
        public string? SavedToFile { get; }

        public DownloadCompletedEventArgs(bool success, string key, long optionalType, byte[]? data, string? savedToFile)
        {
            Success = success;
            Key = key;
            OptionalType = optionalType;
            Data = data;
            SavedToFile = savedToFile;
        }
    }

    public class AllDownloadsCompletedEventArgs : EventArgs
    {
        public bool AllSuccessful { get; }

        public AllDownloadsCompletedEventArgs(bool allSuccessful)
        {
            AllSuccessful = allSuccessful;
        }
    }

    #endregion

    #region Download Entry Class

    private class DownloadEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? SavePath { get; set; }
        public long OptionalType { get; set; }
        public DownloadStatus Status { get; set; } = DownloadStatus.NotYetStarted;
        public byte[]? Data { get; set; }
        public string? Checksum { get; set; }
    }

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, DownloadEntry> _downloads = new();
    private int _activeDownloads = 0;
    private int _maxConcurrentDownloads = 3;

    #endregion

    #region Constructor

    public PdDownload()
    {
        // Invisible at runtime
        IsVisible = false;

        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a URL to the download queue
    /// </summary>
    /// <param name="key">Unique identifier for this download</param>
    /// <param name="url">URL to download from</param>
    /// <param name="savePath">Optional path to save file to</param>
    /// <param name="optionalType">Optional type identifier</param>
    /// <param name="checksum">Optional checksum for verification</param>
    public void AddToQueue(string key, string url, string? savePath = null, long optionalType = 0, string? checksum = null)
    {
        if (_downloads.ContainsKey(key))
        {
            throw new ArgumentException($"Download with key '{key}' already exists in queue.");
        }

        var entry = new DownloadEntry
        {
            Key = key,
            Url = url,
            SavePath = savePath,
            OptionalType = optionalType,
            Checksum = checksum,
            Status = DownloadStatus.NotYetStarted
        };

        _downloads[key] = entry;
    }

    /// <summary>
    /// Start processing the download queue
    /// </summary>
    public async Task StartDownloadsAsync()
    {
        var pendingDownloads = _downloads.Values
            .Where(d => d.Status == DownloadStatus.NotYetStarted)
            .ToList();

        var tasks = pendingDownloads
            .Take(_maxConcurrentDownloads)
            .Select(d => DownloadFileAsync(d));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Cancel a specific download
    /// </summary>
    /// <param name="key">Key of download to cancel</param>
    public void CancelDownload(string key)
    {
        if (_downloads.TryGetValue(key, out var entry))
        {
            entry.Status = DownloadStatus.FailureCallerCanceledDownload;
        }
    }

    /// <summary>
    /// Get downloaded data for a specific key
    /// </summary>
    /// <param name="key">Key of download</param>
    /// <returns>Downloaded data or null if not found/completed</returns>
    public byte[]? GetDownloadedData(string key)
    {
        if (_downloads.TryGetValue(key, out var entry))
        {
            return entry.Data;
        }
        return null;
    }

    /// <summary>
    /// Get status of a specific download
    /// </summary>
    /// <param name="key">Key of download</param>
    /// <returns>Download status</returns>
    public DownloadStatus GetDownloadStatus(string key)
    {
        if (_downloads.TryGetValue(key, out var entry))
        {
            return entry.Status;
        }
        return DownloadStatus.FailureNotTryingAgain;
    }

    /// <summary>
    /// Clear the download queue
    /// </summary>
    public void ClearQueue()
    {
        _downloads.Clear();
        _activeDownloads = 0;
    }

    #endregion

    #region Private Methods

    private async Task DownloadFileAsync(DownloadEntry entry)
    {
        try
        {
            entry.Status = DownloadStatus.Downloading;
            _activeDownloads++;

            // Download the file
            byte[] data = await _httpClient.GetByteArrayAsync(entry.Url);

            // Verify checksum if provided
            if (!string.IsNullOrEmpty(entry.Checksum))
            {
                string downloadedChecksum = CalculateChecksum(data);
                if (!string.Equals(downloadedChecksum, entry.Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    entry.Status = DownloadStatus.FailureChecksumMismatch;
                    RaiseFinishedOneItem(entry, false);
                    return;
                }
            }

            // Save to file if path specified
            if (!string.IsNullOrEmpty(entry.SavePath))
            {
                await File.WriteAllBytesAsync(entry.SavePath, data);
            }

            // Store data
            entry.Data = data;
            entry.Status = DownloadStatus.DownloadComplete;

            RaiseFinishedOneItem(entry, true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Download failed for {entry.Key}: {ex.Message}");
            entry.Status = DownloadStatus.FailureNotTryingAgain;
            RaiseFinishedOneItem(entry, false);
        }
        finally
        {
            _activeDownloads--;

            // Check if all downloads are complete
            CheckAllDownloadsComplete();
        }
    }

    private void RaiseFinishedOneItem(DownloadEntry entry, bool success)
    {
        var args = new DownloadCompletedEventArgs(
            success,
            entry.Key,
            entry.OptionalType,
            entry.Data,
            entry.SavePath);

        FinishedOneItem?.Invoke(this, args);
    }

    private void CheckAllDownloadsComplete()
    {
        if (_activeDownloads == 0)
        {
            bool allSuccessful = _downloads.Values.All(d => d.Status == DownloadStatus.DownloadComplete);
            var args = new AllDownloadsCompletedEventArgs(allSuccessful);
            FinishedAllItems?.Invoke(this, args);
        }
    }

    private string CalculateChecksum(byte[] data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    #endregion

    #region IDisposable

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler == null)
        {
            _httpClient?.Dispose();
        }
    }

    #endregion
}
