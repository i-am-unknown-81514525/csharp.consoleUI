using System;
using System.Collections.Generic;
using System.Linq;

namespace ui.utils
{
    public class ByteBuffer
    {
        private byte[] _buf;

        public ByteBuffer(byte[] buf)
        {
            _buf = (byte[])buf.Clone();
        }

        public ByteBuffer(IEnumerable<byte> buf)
        {
            _buf = buf.ToArray();
        }

        public List<byte> AsList() => _buf.ToList();

        public byte[] AsArray() => _buf.ToArray();

        public char[] AsCharArray() => _buf.Select(x => (char)x).ToArray();

        public string AsString() => _buf.Aggregate("", (prev, curr) => prev + (char)curr);

        public ByteBuffer Clone()
        {
            return new ByteBuffer((byte[])_buf.Clone());
        }

        public int length { get => _buf.Length; }
        public int count { get => _buf.Length; }

        public static ByteBuffer operator +(ByteBuffer curr, IEnumerable<char> other)
        {
            return curr + other.Select(x => (byte)x);
        }

        public static ByteBuffer operator +(ByteBuffer curr, IEnumerable<byte> other)
        {
            return new ByteBuffer(curr.AsArray().Concat(other.ToArray()));
        }

        public static ByteBuffer operator +(ByteBuffer curr, ByteBuffer other)
        {
            return new ByteBuffer(curr.AsArray().Concat(other.AsArray()));
        }
    }

    public static class ByteBufferExtension
    {
        public static ByteBuffer AsByteBuffer(this IEnumerable<char> ori) => new ByteBuffer(
            ori.Select(v => (byte)v)
        );

        public static ByteBuffer AsByteBuffer(this byte[] ori) => new ByteBuffer(ori);

        public static ByteBuffer AsByteBuffer(this IEnumerable<byte> ori) => new ByteBuffer(ori.ToArray());
    }


}