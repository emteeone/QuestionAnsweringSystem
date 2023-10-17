using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Domain.Model
{
    public interface IVectorRepository
    {
        bool DoesDataExists(string prefix);

        Task InsertAsync(string indexName, string prefix, IReadOnlyList<string> textFragments, Func<string, Task<float[]>> embeddingFunc, Func<string, Task<IReadOnlyList<int>>> tokenFunc);

        Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vectorAsBytes);
    }
}
