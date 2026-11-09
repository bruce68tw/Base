using Base.Services;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;

namespace BaseAI.Services
{
    /// <summary>完整 WAV 驗證、PCM 封裝與音訊解碼。</summary>
    public static class _Voice
    {
        private const int MaxAudioBytes = 10 * 1024 * 1024;

        /// <summary>
        /// 轉成完整 WAV。WAV 僅驗證；PCM 必須明示格式；壓縮音訊由 FFmpeg 解碼為 16 kHz 單聲道 16-bit PCM WAV。
        /// 輸入與輸出各限 10 MB，轉碼限 30 秒。失敗或取消時以 _Log.Error 記錄安全訊息並回傳 null。FFmpeg 須由部署環境提供，不會自動下載或安裝。
        /// </summary>
        /// <param name="audioData">完整音檔或完整 PCM 取樣資料。</param>
        /// <param name="mimeType">實際格式，例如 audio/wav、audio/webm、audio/mpeg、audio/pcm。</param>
        /// <param name="sampleRate">裸 PCM 必填，8,000 至 192,000 Hz；不從 MIME 參數猜測。</param>
        /// <param name="channels">裸 PCM 必填，1 至 8 聲道，交錯排列。</param>
        /// <param name="bitsPerSample">裸 PCM 必填，8/16/24/32；8-bit 無號，其餘為 little-endian 有號整數。</param>
        /// <param name="ffmpegPath">FFmpeg 執行檔路徑或 PATH 上的名稱；僅壓縮格式需要。</param>
        /// <param name="ct">取消驗證或轉碼。</param>
        public static async Task<byte[]?> ToWavA(ReadOnlyMemory<byte> audioData, string mimeType,
            int? sampleRate = null, int? channels = null, int? bitsPerSample = null,
            string ffmpegPath = "ffmpeg", CancellationToken ct = default)
        {
            try
            {
                return await ToWavCoreA(audioData, mimeType, sampleRate, channels, bitsPerSample, ffmpegPath, ct);
            }
            catch (Exception error)
            {
                // 不記錄輸入、檔案路徑、工具 stderr 或原始例外訊息；取消也遵循 null 契約。
                var reason = error switch {
                    OperationCanceledException => "conversion cancelled",
                    TimeoutException => "conversion exceeded 30 seconds",
                    ArgumentException => "invalid audio size, WAV structure, MIME or PCM specifications",
                    NotSupportedException => "unsupported audio MIME type",
                    Win32Exception => "FFmpeg could not start; check executable availability and ffmpegPath",
                    IOException => "temporary audio file or decoder pipe I/O failed",
                    InvalidOperationException => "audio decoding failed, returned no audio or exceeded 10 MB",
                    _ => "audio conversion failed"
                };
                _Log.Error("_Voice.ToWavA: " + reason + ".");
                return null;
            }
        }

        private static async Task<byte[]> ToWavCoreA(ReadOnlyMemory<byte> audioData, string mimeType,
            int? sampleRate, int? channels, int? bitsPerSample, string ffmpegPath, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (audioData.IsEmpty || audioData.Length > MaxAudioBytes)
                throw new ArgumentException("Audio must contain 1 byte to 10 MB.", nameof(audioData));
            ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);
            var mediaType = mimeType.Split(';', 2)[0].Trim().ToLowerInvariant();
            if (mediaType is "audio/wav" or "audio/x-wav" or "audio/wave")
            {
                Validate(audioData.Span);
                return audioData.ToArray();
            }
            if (mediaType == "audio/pcm")
            {
                if (!sampleRate.HasValue || !channels.HasValue || !bitsPerSample.HasValue)
                    throw new ArgumentException("Raw PCM requires sampleRate, channels and bitsPerSample.");
                return WrapPcm(audioData.Span, sampleRate.Value, channels.Value, bitsPerSample.Value);
            }
            var format = mediaType switch
            {
                "audio/webm" => "matroska",
                "audio/ogg" => "ogg",
                "audio/mpeg" or "audio/mp3" => "mp3",
                "audio/mp4" or "audio/m4a" or "audio/x-m4a" => "mov",
                "audio/aac" => "aac",
                "audio/flac" => "flac",
                _ => throw new NotSupportedException("Unsupported audio MIME type.")
            };
            ArgumentException.ThrowIfNullOrWhiteSpace(ffmpegPath);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
            var token = timeoutCts.Token;
            // 使用私有暫存檔讓 MP4 等格式可 seek；結束後必定清理，不採用外部檔名。
            var directory = Directory.CreateTempSubdirectory("baseai-audio-");
            try
            {
                var inputPath = Path.Combine(directory.FullName, "input.audio");
                await File.WriteAllBytesAsync(inputPath, audioData.ToArray(), token);
                using var process = new Process { StartInfo = new ProcessStartInfo {
                    FileName = ffmpegPath, UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                } };
                foreach (var argument in new[] { "-hide_banner", "-loglevel", "error", "-nostdin", "-xerror",
                    "-threads", "1",
                    "-protocol_whitelist", "file,pipe", "-f", format, "-i", inputPath,
                    "-map", "0:a:0", "-vn", "-sn", "-dn", "-ac", "1", "-ar", "16000",
                    "-c:a", "pcm_s16le", "-f", "s16le", "pipe:1" })
                    process.StartInfo.ArgumentList.Add(argument);
                try
                {
                    process.Start();
                    // 並行排空 stderr，避免 pipe 阻塞；不將供應商內容或本機路徑帶入例外。
                    var drainError = DrainErrorA(process.StandardError, token);
                    try
                    {
                        using var pcm = new MemoryStream();
                        var buffer = new byte[8192];
                        int count;
                        while ((count = await process.StandardOutput.BaseStream.ReadAsync(buffer, token)) > 0)
                        {
                            if (pcm.Length + count > MaxAudioBytes - 44)
                                throw new InvalidOperationException("Converted WAV exceeds 10 MB.");
                            await pcm.WriteAsync(buffer.AsMemory(0, count), token);
                        }
                        await process.WaitForExitAsync(token);
                        await drainError;
                        if (process.ExitCode != 0 || pcm.Length == 0)
                            throw new InvalidOperationException("Audio decoding failed or produced no audio.");
                        var wav = FromPcm16Mono16Khz(pcm.ToArray());
                        Validate(wav);
                        return wav;
                    }
                    finally
                    {
                        if (!process.HasExited) process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(CancellationToken.None);
                        // Kill 後等待 pipe 排空，確保無遺留未觀察的非同步工作。
                        try { await drainError; } catch (OperationCanceledException) { }
                    }
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested && timeoutCts.IsCancellationRequested)
                {
                    throw new TimeoutException("Audio conversion exceeded 30 seconds.");
                }
            }
            finally
            {
                directory.Delete(recursive: true);
            }
        }

        private static async Task DrainErrorA(StreamReader reader, CancellationToken ct)
        {
            var buffer = new char[1024];
            while (await reader.ReadAsync(buffer.AsMemory(), ct) > 0) { }
        }

        /// <summary>封裝 16 kHz、單聲道、16-bit little-endian PCM，不做重採樣。</summary>
        public static byte[] FromPcm16Mono16Khz(ReadOnlySpan<byte> pcmData) => WrapPcm(pcmData, 16000, 1, 16);

        private static byte[] WrapPcm(ReadOnlySpan<byte> pcmData, int sampleRate, int channels, int bitsPerSample)
        {
            if (sampleRate is < 8000 or > 192000) throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (channels is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(channels));
            if (bitsPerSample is not (8 or 16 or 24 or 32)) throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
            var blockAlign = channels * (bitsPerSample / 8);
            var padding = pcmData.Length & 1;
            if (pcmData.IsEmpty || pcmData.Length % blockAlign != 0 || pcmData.Length > MaxAudioBytes - 44 - padding)
                throw new ArgumentException("PCM must contain complete sample frames within the WAV size limit.", nameof(pcmData));
            var wavData = new byte[44 + pcmData.Length + padding];
            "RIFF"u8.CopyTo(wavData.AsSpan(0, 4));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(4, 4), (uint)(wavData.Length - 8));
            "WAVEfmt "u8.CopyTo(wavData.AsSpan(8, 8));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(16, 4), 16);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(20, 2), 1);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(22, 2), (ushort)channels);
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(24, 4), (uint)sampleRate);
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(28, 4), (uint)(sampleRate * blockAlign));
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(32, 2), (ushort)blockAlign);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(34, 2), (ushort)bitsPerSample);
            "data"u8.CopyTo(wavData.AsSpan(36, 4));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(40, 4), (uint)pcmData.Length);
            pcmData.CopyTo(wavData.AsSpan(44));
            Validate(wavData);
            return wavData;
        }

        /// <summary>檢查完整 RIFF/WAVE、fmt 與非空 data 區塊，拒絕裸 PCM 與截斷資料。</summary>
        public static void Validate(ReadOnlySpan<byte> audioData)
        {
            if (audioData.Length < 12 || audioData.Length > MaxAudioBytes ||
                !audioData[..4].SequenceEqual("RIFF"u8) || !audioData.Slice(8, 4).SequenceEqual("WAVE"u8) ||
                BinaryPrimitives.ReadUInt32LittleEndian(audioData.Slice(4, 4)) != audioData.Length - 8)
                throw new ArgumentException("Supply a complete RIFF/WAVE file up to 10 MB.", nameof(audioData));

            var hasFormat = false;
            var hasData = false;
            long offset = 12;
            while (offset < audioData.Length)
            {
                if (audioData.Length - offset < 8) throw InvalidWav();
                var chunk = audioData[(int)offset..];
                var size = BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(4, 4));
                var end = offset + 8 + size;
                var next = end + (size & 1);
                if (next > audioData.Length) throw InvalidWav();
                if (chunk[..4].SequenceEqual("fmt "u8))
                {
                    if (size < 16 || BinaryPrimitives.ReadUInt16LittleEndian(chunk.Slice(8, 2)) == 0 ||
                        BinaryPrimitives.ReadUInt16LittleEndian(chunk.Slice(10, 2)) == 0 ||
                        BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(12, 4)) == 0 ||
                        BinaryPrimitives.ReadUInt16LittleEndian(chunk.Slice(20, 2)) == 0)
                        throw InvalidWav();
                    hasFormat = true;
                }
                if (chunk[..4].SequenceEqual("data"u8))
                {
                    if (!hasFormat || size == 0) throw InvalidWav();
                    hasData = true;
                }
                offset = next;
            }
            if (!hasFormat || !hasData) throw InvalidWav();
        }

        private static ArgumentException InvalidWav() =>
            new("WAV must contain valid fmt and non-empty data chunks without truncation.", "audioData");
    }
}
