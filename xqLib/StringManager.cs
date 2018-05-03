using System.Collections.Generic;
using System.Linq;

namespace xqLib
{
    public class StringManager
    {
        private readonly uint _initialOffset;
        private readonly Dictionary<string, uint> _strings;
        private readonly XqManager _xq;

        public StringManager(XqManager xq, uint initialOffset)
        {
            _xq = xq;
            _strings = new Dictionary<string, uint>();
            _initialOffset = initialOffset;
        }

        public void AddString(string alias, string str)
        {
            if (_strings.ContainsKey(alias))
                return;
            var strLength = _xq.AddTextData(_initialOffset, str);
            foreach (var key in _strings.Keys.ToList()) _strings[key] += strLength;
            _strings[alias] = 0;
        }

        public uint GetOffset(string alias)
        {
            return _initialOffset + _strings[alias];
        }
    }
}