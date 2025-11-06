using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public class PipeStringStream
    {
        private Stream _PipeStream;
        private UnicodeEncoding _Encoding = new UnicodeEncoding();

        public PipeStringStream(Stream pipeStream)
        {
            _PipeStream = pipeStream;
        }

        public async Task<int> Write(string value)
        {
            byte[] writeBuffer = _Encoding.GetBytes(value);
            var bufferLen = writeBuffer.Length;

            if (bufferLen > UInt16.MaxValue)
            {
                throw new ArgumentException(nameof(value) + " too big");
            }

            _PipeStream.WriteByte((byte)(bufferLen / 256));
            _PipeStream.WriteByte((byte)(bufferLen % 256));
            await _PipeStream.WriteAsync(writeBuffer, 0, bufferLen);
            await _PipeStream.FlushAsync();

            return bufferLen;
        }

        public async Task<string> Read()
        {
            int bufferLen;
            bufferLen = _PipeStream.ReadByte() * 256;
            bufferLen += _PipeStream.ReadByte();
            var inBuffer = new byte[bufferLen];
            await _PipeStream.ReadAsync(inBuffer, 0, bufferLen);

            return _Encoding.GetString(inBuffer);
        }
    }
}
