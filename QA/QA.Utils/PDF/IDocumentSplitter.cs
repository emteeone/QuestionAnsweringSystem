using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Utils.PDF
{
    public  interface IDocumentSplitter
    {
        IReadOnlyList<string> Split(string filePath);
    }
}
