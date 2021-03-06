﻿using System.IO;
using System.IO.Compression;

namespace Skeleton.Web.Server.Helpers
{
    public class GZipCompressor : ICompressor
    {
        public string ContentEncoding => "gzip";

        public Stream CreateStream(Stream destination)
        {
            return new GZipStream(destination, CompressionLevel.Fastest, leaveOpen: true);
        }
    }
}