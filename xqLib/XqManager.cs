using System.Collections.Generic;
using System.IO;

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
    }
}