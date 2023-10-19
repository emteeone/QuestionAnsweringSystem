// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using QA.Application;
using QA.Domain.Model;
using QA.Infrastruture;
using QA.Infrastruture.Repository;
using QA.Utils.PDF;
using StackExchange.Redis;

Console.WriteLine("Hello, World!");

var serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddHttpClient()
                        .AddSingleton<IOpenAIAPI>(_ => new OpenAIAPI(new APIAuthentication("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", "org-cszwXXYg35EA8srPKblNp2gG")))
                        .AddSingleton<IRedisPersistentConnection>( sp =>
                        {
                            var options = new ConfigurationOptions { EndPoints = { "localhost:6379" } };
                            var connection = ConnectionMultiplexer.Connect(options);

                            var logger = sp.GetRequiredService<ILoggerFactory>()
                                                    .CreateLogger<RedisPersistentConnection>();

                            return new RedisPersistentConnection(connection,logger) ;
                        })
                        .AddSingleton<IVectorRepository, RedisDatabaseRepository>()
                        .AddSingleton<IDocumentSplitter, DocumentSplitter>()
                        .AddSingleton<IMainService, MainService>()
                        .BuildServiceProvider();


   var mainService = serviceProvider.GetRequiredService<IMainService>();
    
    await mainService.CallQuestionAsync(@"C:\Users\mbaye\Downloads\original.pdf", "what's virtual machines");

Console.ReadLine();








