using System;
using System.IO;
using System.IO.Compression;
using Verse;


namespace Revolus.Compressor {
    internal class CompressorStream : Stream {
        private Stream outerStream = null;
        internal Stream stream = null;

        public CompressorStream(string filePath) {
            CompressorSettings settings = CompressorMod.Settings;

            var fileStream = new FileStream(filePath, FileMode.CreateNew);
            if (settings.level < 0) {
                this.stream = fileStream;
            } else {
                this.outerStream = fileStream;
                this.stream = new GZipStream(
                    fileStream,
                    settings.level > 0 ? CompressionLevel.Optimal : CompressionLevel.Fastest
                );
            }
        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush() {
            try {
                this.stream?.Flush();
            } finally {
                this.outerStream?.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override void Close() {
            Stream outerStream = this.outerStream;
            Stream stream = this.stream;
                
            this.outerStream = null;
            this.stream = null;

            try {
                stream?.Close();
            } finally {
                outerStream?.Close();
            }
        }
    }
}
