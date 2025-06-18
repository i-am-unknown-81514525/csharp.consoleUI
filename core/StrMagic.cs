using System;
using System.Collections.Generic;
using System.Linq;

namespace ui.core
{
    public class ByteBuffer
    {
        private byte[] buf;

        public ByteBuffer(byte[] buf)
        {
            this.buf = (byte[])buf.Clone();
        }

        public ByteBuffer(IEnumerable<byte> buf)
        {
            this.buf = buf.ToArray();
        }

        public List<byte> AsList() => buf.ToList();

        public byte[] AsArray() => buf.ToArray();

        public char[] AsCharArray() => buf.Select(x => (char)x).ToArray();

        public string AsString() => buf.Aggregate("", (prev, curr) => prev + (char)curr);

        public ByteBuffer Clone()
        {
            return new ByteBuffer((byte[])buf.Clone());
        }

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