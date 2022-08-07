using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenRGB.NET.Utils
{
    public class BlockingResponseMap
    {
        private readonly Dictionary<uint, BlockingCollection<byte[]>> _map = new();
        private readonly int _timeout;

        /// <summary>
        /// Creates a new BlockingResponseMap with a default timeout of 1 seconds.
        /// </summary>
        /// <param name="timeout"></param>
        public BlockingResponseMap(int timeout = 1000)
        {
            _timeout = timeout;
        }


        /// <summary>
        /// Blocking access for the response map.
        /// </summary>
        /// <param name="key"></param>
        public byte[] this[uint key]
        {
            set
            {
                CreateKey(key);

                _map[key].Add(value);
            }
        }

        /// <summary>
        /// Blocking access for the response map.
        /// </summary>
        /// <param name="key"></param>
        public byte[] this[CommandId key]
        {
            get
            {
                CreateKey(key);

                var tryTake = _map[(uint)key].TryTake(out var result, _timeout);
                if (!tryTake)
                {
                    throw new TimeoutException($"No response for command {key} within {_timeout}ms");
                }
                return result;
            }
        }

        private void CreateKey(uint key)
        {
            if (!_map.ContainsKey(key))
            {
                _map[key] = new BlockingCollection<byte[]>(1);
            }
        }

        private void CreateKey(CommandId key)
        {
            if (!_map.ContainsKey((uint) key))
            {
                _map[(uint) key] = new BlockingCollection<byte[]>(1);
            }
        }
    }
}