using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Domain.Model
{
    public record VectorDocument(int Idx, string Text, int TokenLength, double Score);
}
