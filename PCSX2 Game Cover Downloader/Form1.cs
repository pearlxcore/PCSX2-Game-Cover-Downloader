using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCSX2_Game_Cover_Downloader
{
    public partial class Form1 : Form
    {
        private const int MaxConcurrency = 6;
        private const int MaxRetries = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan ConnectivityTimeout = TimeSpan.FromSeconds(3);

        private CancellationTokenSource _cts;

        public Form1()
        {
            InitializeComponent();
            lblVersion.Text = "v" + GetAppVersion();

            WireUpEvents();

            // initial UI state
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            lblStatus.Text = "...";
        }

        private string GetAppVersion()
        {
            var info = System.Reflection.Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return info ?? "1.0.0";
        }

        private void WireUpEvents()
        {
            // Drag & drop: enable and wire events
            tbGameListLoc.AllowDrop = true;
            tbGameListLoc.DragEnter += FileDragEnter;
            tbGameListLoc.DragDrop += tbGameListLoc_DragDrop;

            tbGameCoverDir.AllowDrop = true;
            tbGameCoverDir.DragEnter += FileDragEnter;
            tbGameCoverDir.DragDrop += tbGameCoverDir_DragDrop;
        }

        private void FileDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void btnGameListLocation_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "PCSX2 Cache File|gamelist.cache|All Files|*.*";
                if (f.ShowDialog() == DialogResult.OK)
                    tbGameListLoc.Text = f.FileName;
            }
        }

        private void btnGameCoverDir_Click(object sender, EventArgs e)
        {
            using (var f = new FolderBrowserDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                    tbGameCoverDir.Text = f.SelectedPath;
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            // Clear log every download attempt
            lbLog.Items.Clear();

            // Disable UI while running
            btnDownload.Enabled = false;
            btnBrowseCache.Enabled = false;
            btnBrowseCover.Enabled = false;
            tbGameListLoc.Enabled = false;
            tbGameCoverDir.Enabled = false;
            cbOverwrite.Enabled = false;
            progressBar1.Value = 0;
            lblStatus.Text = "Preparing...";

            // Read user input (may be empty)
            string userCacheInput = tbGameListLoc.Text.Trim();
            string userCoverInput = tbGameCoverDir.Text.Trim();

            // Default PCSX2 locations
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string defaultPcsx2 = Path.Combine(docs, "PCSX2");
            string defaultCache = Path.Combine(defaultPcsx2, "cache", "gamelist.cache");
            string defaultCovers = Path.Combine(defaultPcsx2, "covers");

            // Decide final paths (prefer user input; fall back to defaults)
            string cachePath = string.IsNullOrWhiteSpace(userCacheInput) ? defaultCache : userCacheInput;
            string coverFolder = string.IsNullOrWhiteSpace(userCoverInput) ? defaultCovers : userCoverInput;

            // If user didn't provide either path, ensure at least defaults exist (otherwise stop)
            bool userProvidedCache = !string.IsNullOrWhiteSpace(userCacheInput);
            bool userProvidedCover = !string.IsNullOrWhiteSpace(userCoverInput);

            // If neither provided and neither default exists -> stop
            if (!userProvidedCache && !userProvidedCover)
            {
                bool defaultCacheExists = File.Exists(defaultCache);
                bool defaultCoversExists = Directory.Exists(defaultCovers);

                if (!defaultCacheExists && !defaultCoversExists)
                {
                    MessageBox.Show(
                        $"Neither cache nor cover directory provided, and default locations were not found:\n\n" +
                        $"Default cache: {defaultCache}\nDefault covers: {defaultCovers}\n\n" +
                        "Please either provide the gamelist.cache path and/or the cover folder, or place them in the default PCSX2 Documents folder.",
                        "Missing Inputs",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    RestoreUi();
                    return;
                }
            }

            // If user provided cache but not cover, try to use/create default cover folder
            if (userProvidedCache && !userProvidedCover)
            {
                // coverFolder already set to defaultCovers
                try
                {
                    if (!Directory.Exists(coverFolder))
                        Directory.CreateDirectory(coverFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot create default cover folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RestoreUi();
                    return;
                }
            }

            // If user provided cover but not cache, check default cache exists; if not, stop
            if (!userProvidedCache && userProvidedCover)
            {
                if (!File.Exists(cachePath))
                {
                    MessageBox.Show(
                        $"You provided a cover directory but no cache file was provided and the default cache was not found:\n\n{cachePath}\n\n" +
                        "Please provide a valid gamelist.cache file.",
                        "Missing Cache",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    RestoreUi();
                    return;
                }
            }

            // If both provided by user, just validate existence / create folder if needed
            if (userProvidedCache && userProvidedCover)
            {
                if (!File.Exists(cachePath))
                {
                    MessageBox.Show($"Provided cache file not found:\n{cachePath}", "Missing Cache", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RestoreUi();
                    return;
                }

                try
                {
                    if (!Directory.Exists(coverFolder))
                        Directory.CreateDirectory(coverFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot create/open cover folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RestoreUi();
                    return;
                }
            }

            // At this point cachePath should point to an existing file or we already returned.
            if (!File.Exists(cachePath))
            {
                MessageBox.Show($"Cache file not found:\n{cachePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RestoreUi();
                return;
            }

            // Lightweight internet connectivity check BEFORE extracting serials and starting downloads
            bool online = await CheckInternetConnectivityAsync();
            if (!online)
            {
                MessageBox.Show("No internet connection detected. Please connect to the internet and try again.", "No Internet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RestoreUi();
                return;
            }

            // extract serials
            List<string> serials;
            try
            {
                serials = SerialExtractor.FromFile(cachePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read cache: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RestoreUi();
                return;
            }

            if (serials == null || serials.Count == 0)
            {
                MessageBox.Show("No serials found in the cache file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RestoreUi();
                return;
            }

            progressBar1.Maximum = serials.Count;
            progressBar1.Value = 0;
            lblStatus.Text = $"0 / {serials.Count}";

            bool overwrite = cbOverwrite.Checked;

            _cts = new CancellationTokenSource();
            var progress = new Progress<DownloadProgress>(OnProgress);

            try
            {
                await DownloadAllCoversAsync(serials, coverFolder, overwrite, _cts.Token, progress);
                AddLog("All tasks finished.");
            }
            catch (OperationCanceledException)
            {
                AddLog("Download cancelled.");
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
                MessageBox.Show($"Error during download: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                RestoreUi();
                _cts = null;
            }
        }

        private void RestoreUi()
        {
            btnDownload.Enabled = true;
            btnBrowseCache.Enabled = true;
            btnBrowseCover.Enabled = true;
            tbGameListLoc.Enabled = true;
            tbGameCoverDir.Enabled = true;
            cbOverwrite.Enabled = true;
        }

        private void OnProgress(DownloadProgress p)
        {
            // update UI: progress and log
            if (p.Completed >= 0)
            {
                progressBar1.Value = Math.Min(progressBar1.Maximum, p.Completed);
                lblStatus.Text = $"{p.Completed} / {progressBar1.Maximum}   ({p.CurrentSerial ?? "-"})";
            }

            if (!string.IsNullOrEmpty(p.Message))
                AddLog(p.Message);
        }

        private void AddLog(string message)
        {
            if (lbLog.InvokeRequired)
            {
                lbLog.Invoke(new Action(() => AddLog(message)));
                return;
            }

            string ts = DateTime.Now.ToString("HH:mm:ss");
            lbLog.Items.Add($"[{ts}] {message}");
            // autoscroll
            lbLog.TopIndex = Math.Max(0, lbLog.Items.Count - 1);
        }

        private async Task DownloadAllCoversAsync(List<string> serials, string outFolder, bool overwrite, CancellationToken token, IProgress<DownloadProgress> progress)
        {
            using var http = new HttpClient();
            var sem = new SemaphoreSlim(MaxConcurrency);
            var tasks = new List<Task>();
            int completed = 0;

            string urlTemplate = "https://raw.githubusercontent.com/xlenore/ps2-covers/main/covers/default/{0}.jpg";

            foreach (var serial in serials)
            {
                token.ThrowIfCancellationRequested();
                await sem.WaitAsync(token);

                var s = serial; // capture
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var res = await DownloadSingleCoverAsync(http, string.Format(urlTemplate, Uri.EscapeDataString(s)), s, outFolder, overwrite, token);
                        string msg = res switch
                        {
                            DownloadResult.Ok => $"[OK]   {s}",
                            DownloadResult.Skipped => $"[SKIP] {s} (exists)",
                            DownloadResult.NotFound => $"[404]  {s}",
                            DownloadResult.Failed => $"[FAIL] {s}",
                            _ => $"[?]    {s}"
                        };

                        int done = Interlocked.Increment(ref completed);
                        progress.Report(new DownloadProgress { Completed = done, CurrentSerial = s, Message = msg });
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, token));
            }

            await Task.WhenAll(tasks);
        }

        private enum DownloadResult { Ok, Skipped, NotFound, Failed }

        private async Task<DownloadResult> DownloadSingleCoverAsync(HttpClient http, string url, string serial, string outFolder, bool overwrite, CancellationToken token)
        {
            string dest = Path.Combine(outFolder, $"{serial}.jpg");
            if (File.Exists(dest) && !overwrite)
                return DownloadResult.Skipped;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    using var resp = await http.GetAsync(url, token);
                    if (resp.IsSuccessStatusCode)
                    {
                        var bytes = await resp.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(dest, bytes, token);
                        return DownloadResult.Ok;
                    }
                    else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return DownloadResult.NotFound;
                    }
                    else
                    {
                        // transient HTTP error - retry
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch
                {
                    // swallow and retry
                }

                await Task.Delay(RetryDelay, token);
            }

            return DownloadResult.Failed;
        }

        private class DownloadProgress
        {
            public int Completed { get; set; } = -1;
            public string CurrentSerial { get; set; }
            public string Message { get; set; }
        }

        private void tbGameListLoc_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && File.Exists(files[0]))
                tbGameListLoc.Text = files[0];
        }

        private void tbGameCoverDir_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Directory.Exists(files[0]))
                tbGameCoverDir.Text = files[0];
        }

        /// <summary>
        /// Lightweight internet connectivity check using HEAD to raw.githubusercontent.com
        /// </summary>
        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var client = new HttpClient() { Timeout = ConnectivityTimeout };
                using var req = new HttpRequestMessage(HttpMethod.Head, "https://raw.githubusercontent.com/");
                using var resp = await client.SendAsync(req);
                return resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.Forbidden || resp.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        // Cancel running downloads when form closes
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            base.OnFormClosing(e);
        }
    }

    // -------------------- SerialExtractor helper class --------------------
    public static class SerialExtractor
    {
        private static readonly Regex SerialRegex = new Regex(@"\b([A-Za-z]{4}-[0-9A-Za-z]{5})\b", RegexOptions.Compiled);
        private static readonly Regex PrefixAlpha = new Regex(@"^[A-Z]{4}$", RegexOptions.Compiled);

        public static List<string> FromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Cache file not found", path);
            byte[] bytes = File.ReadAllBytes(path);
            return FromBytes(bytes);
        }

        public static List<string> FromBytes(byte[] data)
        {
            string text = Encoding.Latin1.GetString(data);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in SerialRegex.Matches(text))
            {
                string candidate = m.Groups[1].Value.ToUpperInvariant();
                if (candidate.Length >= 4 && PrefixAlpha.IsMatch(candidate.Substring(0, 4)))
                    set.Add(candidate);
            }

            var list = new List<string>(set);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }
    }
}
