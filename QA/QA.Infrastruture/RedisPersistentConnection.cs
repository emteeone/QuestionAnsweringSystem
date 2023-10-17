using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Infrastruture
{
    public class RedisPersistentConnection : IRedisPersistentConnection
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisPersistentConnection> _logger;
        private bool _disposed;
        public RedisPersistentConnection(IConnectionMultiplexer connectionMultiplexer,
                                        ILogger<RedisPersistentConnection> logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connectionMultiplexer.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public bool IsConnected
        {
            get
            {
                return _connectionMultiplexer != null && _connectionMultiplexer.IsConnected;
            }
        }

        public IDatabase GetDatabase()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No Redis connections are available to perform this action");
            }

            return _connectionMultiplexer.GetDatabase();    
        }
    }
}
