using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Utils.Text
{
    public abstract class TextSplitter
    {
        protected int ChunkSize;
        protected int ChunkOverlap;
        protected Func<string, int> LengthFunction;

        protected TextSplitter(int chunkSize = 4000, 
                               int chunkOverlap = 200, 
                               Func<string, int>? lengthFunction = null)
        {
            if (chunkOverlap > chunkSize)
            {
                throw new ArgumentException($"Got a larger chunk overlap ({chunkOverlap}) than chunk size ({chunkSize}), should be smaller.");
            }

            ChunkSize = chunkSize;
            ChunkOverlap = chunkOverlap;
            LengthFunction = lengthFunction ?? new Func<string, int>(text => text.Length);
        }

        public abstract IReadOnlyList<string> SplitText(string text);
    }
}
