using System;

namespace GroBuf
{
    internal class InternalSerializer : IGroBufCustomSerializer
    {
        public InternalSerializer(GroBufWriter groBufWriter, GroBufReader groBufReader, Type type, bool ignoreCustomSerializer)
        {
            this.groBufWriter = groBufWriter;
            this.groBufReader = groBufReader;
            this.type = type;
            this.ignoreCustomSerializer = ignoreCustomSerializer;
        }

        public int CountSize(object obj, bool writeEmpty)
        {
            return groBufWriter.GetSize(type, ignoreCustomSerializer, obj, writeEmpty);
        }

        public void Write(object obj, bool writeEmpty, IntPtr result, ref int index)
        {
            groBufWriter.Write(type, ignoreCustomSerializer, obj, writeEmpty, result, ref index);
        }

        public void Read(IntPtr data, ref int index, int length, ref object result)
        {
            groBufReader.Read(type, ignoreCustomSerializer, data, ref index, length, ref result);
        }

        private readonly GroBufWriter groBufWriter;
        private readonly GroBufReader groBufReader;
        private readonly Type type;
        private readonly bool ignoreCustomSerializer;
    }
}