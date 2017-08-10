using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FileOptics
{
    public delegate void Compute(byte[] data, uint length, UInt32[] state);
    public delegate void ComputeBlocks(byte[] data, uint length, UInt32[] state);
    public delegate void Finalize(byte[] data, uint length, ulong total, UInt32[] state);

    public class NayukiSHA256
    {
        public const int HASH_BUFFER = 1024 * 512; //512KB
        public const UInt32
            SHA256_IV_0 = 0x6A09E667,
            SHA256_IV_1 = 0xBB67AE85,
            SHA256_IV_2 = 0x3C6EF372,
            SHA256_IV_3 = 0xA54FF53A,
            SHA256_IV_4 = 0x510E527F,
            SHA256_IV_5 = 0x9B05688C,
            SHA256_IV_6 = 0x1F83D9AB,
            SHA256_IV_7 = 0x5BE0CD19;

        protected ulong total = 0;
        protected int buffered = 0;
        protected byte[] buffer = null;

        protected UInt32[] cstate;

        protected Compute _Compute;
        protected ComputeBlocks _ComputeBlocks;
        protected Finalize _Finalize;

#if !_X64
        [DllImport("nayuki-sha256-w32.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256_finalize(byte[] data, uint length, ulong total, UInt32[] state);
        [DllImport("nayuki-sha256-w32.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256(byte[] data, uint length, UInt32[] state);
        [DllImport("nayuki-sha256-w32.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256_computeblocks(byte[] data, uint length, UInt32[] state);
#else
        [DllImport("nayuki-sha256-w64.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256_finalize(byte[] data, uint length, ulong total, UInt32[] state);
        [DllImport("nayuki-sha256-w64.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256(byte[] data, uint length, UInt32[] state);
        [DllImport("nayuki-sha256-w64.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void nayuki_io_sha256_computeblocks(byte[] data, uint length, UInt32[] state);
#endif

        public NayukiSHA256(int buffersize = 1024 * 500)
        {
            if (buffersize % 64 != 0)
                throw new ArgumentException("Buffer size must be divisible by 64");

            cstate = new UInt32[] {
                SHA256_IV_0,
                SHA256_IV_1,
                SHA256_IV_2,
                SHA256_IV_3,
                SHA256_IV_4,
                SHA256_IV_5,
                SHA256_IV_6,
                SHA256_IV_7
            };

            buffer = new byte[buffersize];

            this._Compute = nayuki_io_sha256;
            this._ComputeBlocks = nayuki_io_sha256_computeblocks;
            this._Finalize = nayuki_io_sha256_finalize;
        }

        public static void ReverseEndianness(ref UInt32[] hash)
        {
            for (int i = 0; i < hash.Length; i++)
            {
                hash[i] =
                   (hash[i] & 0x000000FFU) << 0x18 | (hash[i] & 0x0000FF00U) << 0x08 |
                   (hash[i] & 0x00FF0000U) >> 0x08 | (hash[i] & 0xFF000000U) >> 0x18;
            }
        }

        public void Append(byte[] data, int offset, int length)
        {
            if (finalized)
                throw new Exception("Hash has already been finalized");

            if (length < 1)
                length = data.Length - offset;

            int end = offset + length;

            total += (uint)length;

            int remaining = 0;
            for (int i = offset; i < end; i += buffer.Length)
            {
                if (i > end)
                    break;

                remaining = end - i;
                if (remaining + buffered <= buffer.Length)
                {
                    Array.Copy(data, i, buffer, buffered, remaining);
                    buffered += remaining;

                    if (buffered < buffer.Length)
                        break;
                }
                else if (buffered > 0) //Should only fire either once or never
                {
                    int tob = buffered - buffer.Length;
                    Array.Copy(data, i, buffer, buffered, tob);
                    i += tob;
                }
                else
                    Array.Copy(data, i, buffer, 0, buffer.Length);

                //Should only reach this point if the buffer is FULL!!!
                _ComputeBlocks(buffer, (uint)buffer.Length, cstate);
                Array.Clear(buffer, 0, buffer.Length);
                buffered = 0;
            }
        }

        bool finalized = false;

        byte[] _Hash = null;
        public byte[] Hash
        {
            get { return _Hash; }
        }

        public void Finalize()
        {
            if (finalized)
                throw new Exception("Hash has already been finalized");

            //if (Root.x32 && total > Int32.MaxValue)
            //    throw new UnrecoverableException("Total size exceeds the largest size of a 32bit integer.\r\nIf possible try building or running the 64bit version instead.");

            _Finalize(buffer, (uint)buffered, total, cstate);

            finalized = true;

            ReverseEndianness(ref cstate);

            _Hash = new byte[cstate.Length * 4];
            int hashindex = 0;
            for (int i = 0; i < cstate.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(cstate[i]);
                for (int j = 0; j < 4; j++)
                {
                    _Hash[hashindex] = bytes[j];
                    hashindex++;
                }
            }
        }
        public void Finalize(byte[] data, int length)
        {
            if (data.Length > 0)
                Append(data, 0, length);
            Finalize();
        }

        public void AppendStream(Stream s, bool finalize)
        {
            if (!s.CanRead)
                throw new ArgumentException("Stream must be readable");

            byte[] sbuffer = new byte[buffer.Length];
            int read = 0;
            while ((read = s.Read(sbuffer, 0, (buffered > 0 ? buffer.Length - buffered : buffer.Length))) > 0)
                this.Append(sbuffer, 0, read);
            if (finalize)
                this.Finalize();
        }
        public void AppendFile(string file, bool finalize = true)
        {
            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                AppendStream(fs, finalize);
        }

        public static byte[] Calculate(byte[] data)
        {
            UInt32[] cstate = new UInt32[] {
                SHA256_IV_0,
                SHA256_IV_1,
                SHA256_IV_2,
                SHA256_IV_3,
                SHA256_IV_4,
                SHA256_IV_5,
                SHA256_IV_6,
                SHA256_IV_7
            };
            nayuki_io_sha256(data, (uint)data.Length, cstate);

            ReverseEndianness(ref cstate);
            
            byte[] hash = new byte[cstate.Length * 4];
            int hashindex = 0;
            for (int i = 0; i < cstate.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(cstate[i]);
                for (int j = 0; j < 4; j++)
                {
                    hash[hashindex] = bytes[j];
                    hashindex++;
                }
            }

            return hash;
        }
    }
}
