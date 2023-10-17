using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Infrastruture
{
    public interface IRedisPersistentConnection
    {
        bool IsConnected { get; }
        IDatabase GetDatabase();

    }
}
