using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xqLib
{
    public class XqManager
    {
        private readonly XqInternal _xq;

        public XqManager(XqInternal xq)
        {
            _xq = xq;
        }

        public static XqManager FromStream(Stream stream)
        {
            var xq = new XqInternal(stream);
            return new XqManager(xq);
        }

        public List<string> GetDebugData()
        {
            return _xq.GetDebugData();
        }

        public void Save(Stream file)
        {
            _xq.Save(file);
        }

        public void AddTextData(int offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            _xq.t4_data.InsertRange(offset, bytes);

            _xq.RealignTextOffsets(offset, bytes.Length);
        }

        public int DeleteTextData(int offset, bool realign = true)
        {
            var count = 0;
            while (_xq.t4_data[offset] != '\0')
            {
                _xq.t4_data.RemoveAt(offset);
                count++;
            }

            _xq.t4_data.RemoveAt(offset);
            count++;

            if (realign)
                _xq.RealignTextOffsets(offset, -count);

            return count;
        }

        public void ReplaceTextData(int offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            var old_len = DeleteTextData(offset, false);
            var diff = bytes.Length - old_len;

            _xq.t4_data.InsertRange(offset, bytes);
            _xq.RealignTextOffsets(offset, diff, true);
        }
    }
}