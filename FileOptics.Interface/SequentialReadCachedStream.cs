using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.Interface
{
    internal class SequentialReadCachedStream : Stream
    {
        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return false; } }

        int CacheSize;
        public SequentialReadCachedStream(Stream s, int CacheSize = 1024 * 512) //0.5 MB default cache size
        {
            if (!s.CanRead)
                throw new ArgumentException("Stream must be readable.");
            if (!s.CanSeek)
                throw new ArgumentException("Stream must be seekable.");
        }

        protected override void Dispose(bool disposing)
        {
            //base.Dispose(disposing);
        }

#region Write functions
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
#endregion

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
