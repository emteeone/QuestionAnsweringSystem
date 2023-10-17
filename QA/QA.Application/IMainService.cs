using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Application
{
    public interface IMainService
    {
        Task CallQuestionAsync(string filePath, string question);
    }
}
