using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for compressing HTTP responses.
    /// </summary>
    public class CompressionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CompressionMiddleware> _logger;
        private readonly CompressionOptions _options;

        public CompressionMiddleware(RequestDelegate next, ILogger<CompressionMiddleware> logger, CompressionOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var request = context.Request;
            var response = context.Response;

            // Check if compression is supported
            if (!IsCompressionSupported(request))
            {
                await _next(context);
                return;
            }

            // Check if response should be compressed
            if (!ShouldCompressResponse(request, response))
            {
                await _next(context);
                return;
            }

            // Get compression type
            var compressionType = GetCompressionType(request);
            if (compressionType == CompressionType.None)
            {
                await _next(context);
                return;
            }

            // Compress response
            await CompressResponseAsync(context, compressionType);
        }

        private bool IsCompressionSupported(HttpRequest request)
        {
            var acceptEncoding = request.Headers.AcceptEncoding.ToString();
            return acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate") || acceptEncoding.Contains("br");
        }

        private bool ShouldCompressResponse(HttpRequest request, HttpResponse response)
        {
            // Check if response is already compressed
            if (response.Headers.ContainsKey("Content-Encoding"))
            {
                return false;
            }

            // Check content type
            var contentType = response.ContentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            // Check if content type is compressible
            if (!_options.CompressibleContentTypes.Any(ct => contentType.Contains(ct)))
            {
                return false;
            }

            // Check minimum content length
            if (response.ContentLength.HasValue && response.ContentLength.Value < _options.MinimumContentLength)
            {
                return false;
            }

            // Check if response is an error
            if (response.StatusCode >= 400)
            {
                return false;
            }

            return true;
        }

        private CompressionType GetCompressionType(HttpRequest request)
        {
            var acceptEncoding = request.Headers.AcceptEncoding.ToString().ToLowerInvariant();

            if (acceptEncoding.Contains("br") && _options.EnableBrotli)
            {
                return CompressionType.Brotli;
            }
            else if (acceptEncoding.Contains("gzip") && _options.EnableGzip)
            {
                return CompressionType.Gzip;
            }
            else if (acceptEncoding.Contains("deflate") && _options.EnableDeflate)
            {
                return CompressionType.Deflate;
            }

            return CompressionType.None;
        }

        private async Task CompressResponseAsync(HttpContext context, CompressionType compressionType)
        {
            var response = context.Response;
            var originalBodyStream = response.Body;

            try
            {
                using var compressedStream = new MemoryStream();
                response.Body = compressedStream;

                await _next(context);

                // Check if response was written
                if (compressedStream.Length == 0)
                {
                    return;
                }

                // Compress the response
                var compressedData = await CompressDataAsync(compressedStream.ToArray(), compressionType);
                
                // Set compression headers
                SetCompressionHeaders(response, compressionType);
                response.ContentLength = compressedData.Length;

                // Write compressed data to original stream
                await originalBodyStream.WriteAsync(compressedData, 0, compressedData.Length);
            }
            finally
            {
                response.Body = originalBodyStream;
            }
        }

        private async Task<byte[]> CompressDataAsync(byte[] data, CompressionType compressionType)
        {
            using var outputStream = new MemoryStream();
            
            switch (compressionType)
            {
                case CompressionType.Gzip:
                    using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await gzipStream.WriteAsync(data, 0, data.Length);
                    }
                    break;

                case CompressionType.Deflate:
                    using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await deflateStream.WriteAsync(data, 0, data.Length);
                    }
                    break;

                case CompressionType.Brotli:
                    using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        await brotliStream.WriteAsync(data, 0, data.Length);
                    }
                    break;

                default:
                    return data;
            }

            return outputStream.ToArray();
        }

        private void SetCompressionHeaders(HttpResponse response, CompressionType compressionType)
        {
            var contentEncoding = compressionType switch
            {
                CompressionType.Gzip => "gzip",
                CompressionType.Deflate => "deflate",
                CompressionType.Brotli => "br",
                _ => throw new ArgumentException($"Unsupported compression type: {compressionType}")
            };

            response.Headers.Add("Content-Encoding", contentEncoding);
            response.Headers.Add("Vary", "Accept-Encoding");
        }
    }

    /// <summary>
    /// Compression type enumeration.
    /// </summary>
    public enum CompressionType
    {
        None,
        Gzip,
        Deflate,
        Brotli
    }

    /// <summary>
    /// Compression configuration options.
    /// </summary>
    public class CompressionOptions
    {
        /// <summary>
        /// Whether compression is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether Gzip compression is enabled.
        /// </summary>
        public bool EnableGzip { get; set; } = true;

        /// <summary>
        /// Whether Deflate compression is enabled.
        /// </summary>
        public bool EnableDeflate { get; set; } = true;

        /// <summary>
        /// Whether Brotli compression is enabled.
        /// </summary>
        public bool EnableBrotli { get; set; } = true;

        /// <summary>
        /// Minimum content length to compress.
        /// </summary>
        public int MinimumContentLength { get; set; } = 1024; // 1KB

        /// <summary>
        /// Content types that should be compressed.
        /// </summary>
        public HashSet<string> CompressibleContentTypes { get; set; } = new()
        {
            "text/",
            "application/json",
            "application/xml",
            "application/javascript",
            "application/x-javascript",
            "text/css",
            "text/javascript",
            "text/xml",
            "text/plain",
            "text/html",
            "application/atom+xml",
            "application/rss+xml",
            "application/xhtml+xml",
            "image/svg+xml"
        };

        /// <summary>
        /// Compression level.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
    }
}