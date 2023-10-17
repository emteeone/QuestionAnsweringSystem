using Microsoft.Extensions.Logging;
using OpenAI_API.Embedding;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QA.Application
{
    public static partial class OpenAIAPIExtensions
    {
        private const int DefaultTimeOutInSeconds = 20;
        private const int MaxRetries = 10;
        private static readonly Regex RateLimitReachedRegex = new("Rate limit reached", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex PleaseRetryYourRequestRegex = new("Please retry your request", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex TooManyRequestsRegex = new("TooManyRequests", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex PleaseTryAgainRegex = new(@"Please try again in (\d+)s\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly AsyncRetryPolicy AsyncRetryPolicy = Policy
       .Handle<HttpRequestException>(IsExceptionRetryable)
       .WaitAndRetryAsync(MaxRetries, SleepDurationProvider, OnRetryAsync);

        private static bool IsExceptionRetryable(HttpRequestException httpException)
        {
            return RateLimitReachedRegex.IsMatch(httpException.Message) ||
                   PleaseRetryYourRequestRegex.IsMatch(httpException.Message) ||
                   TooManyRequestsRegex.IsMatch(httpException.Message);
        }

        private static bool TryExtractWaitSecondsFromExceptionMessage(string exceptionMessage, out int waitSeconds)
        {
            var match = PleaseTryAgainRegex.Match(exceptionMessage);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var parsedValue))
            {
                waitSeconds = parsedValue;
                return true;
            }

            waitSeconds = default;
            return false;
        }

        private static TimeSpan SleepDurationProvider(int retryAttempt, Exception exception, Context context)
        {
            var seconds = TryExtractWaitSecondsFromExceptionMessage(exception.Message, out var waitSeconds)
                ? waitSeconds
                : DefaultTimeOutInSeconds;

            if (seconds == 1)
            {
                seconds = 2;
            }

            return retryAttempt == 1 ? TimeSpan.FromSeconds(seconds) : TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        }

        private static Task OnRetryAsync(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            OnRetry(exception, timeSpan, retryCount, context);
            return Task.CompletedTask;
        }

        private static void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
        {
            //Logger.Value.LogDebug(exception, "Request failed. Waiting {timeSpan} before next retry. Retry attempt {retryCount}/{maxRetries}.", timeSpan, retryCount, MaxRetries);
        }
        /// <summary>
        /// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
        /// <param name="endpoint">The endpoint on which the function is executed.</param>
        /// <param name="func">The asynchronous function to be executed with retry logic.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task<TResult> WithRetry<TResult>(this IEmbeddingEndpoint endpoint, Func<IEmbeddingEndpoint, Task<TResult>> func)
        {
            return AsyncRetryPolicy.ExecuteAsync(() => func(endpoint));
        }
    }
}
