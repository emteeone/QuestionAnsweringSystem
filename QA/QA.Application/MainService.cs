using Microsoft.Extensions.Logging;
using OpenAI_API;
using Polly;
using Polly.Retry;
using QA.Domain.Model;
using QA.Utils.PDF;
using SharpToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QA.Application
{
    public class MainService : IMainService
    {
        private const int MaxTokenLength = 4096;
        private const int MaxTokenLengthGpt4 = 8192;
        private readonly GptEncoding _encoding = GptEncoding.GetEncoding("cl100k_base");
        private const string NullAnswer = "NULL";

        private readonly ILogger<MainService> _logger;
        private readonly IDocumentSplitter _documentSplitter;
        private readonly IVectorRepository _dataInserter;
        private readonly IOpenAIAPI _openAiAPI;

        public MainService(IVectorRepository dataInserter,
                            ILogger<MainService> logger,
                            IDocumentSplitter documentSplitter,
                            IOpenAIAPI openAiAPI)
        {
            _documentSplitter = documentSplitter;
            _dataInserter = dataInserter;
            _logger = logger;
            _openAiAPI = openAiAPI;
        }

        public Task CallQuestionAsync(string filePath, string question)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> CanUseGPT4Async()
        {
            var policy = RetryPolicy.Handle<SocketException>()
               .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   _logger.LogWarning(ex, "Could not get OpenAPI model ({ExceptionMessage})", ex.Message);
               });

            var models =  await  policy.ExecuteAsync(async () =>
                            {
                                return await _openAiAPI.Models.GetModelsAsync();
                            });


            return models.Any(m => m.ModelID == "gpt-4");
        }

        private async Task AddDataIfNotExistsAsync(string filePath, string indexName, string prefix)
        {
            if (_dataInserter.DoesDataExists(prefix))
            {
                return;
            }

            var textFragments = _documentSplitter.Split(filePath);

            await _dataInserter.InsertAsync(
                indexName: indexName,
                prefix: prefix,
                textFragments,
                embeddingFunc: input => _openAiAPI.Embeddings.WithRetry(embeddings => embeddings.GetEmbeddingsAsync(input)),
                tokenFunc: async input => await Task.Run(() => _encoding.Encode(input))
            );
        }

         
    }
}
