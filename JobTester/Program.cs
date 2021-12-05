using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

using SampleJob;

namespace JobTester
{
    class Program
    {
        private readonly ILogger<Program> _logger;

        public Program(ILogger<Program> logger)
        {
            this._logger = logger;
        }
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().Build();
            var logger = LogManager.Setup()
                                   .SetupExtensions(ext => ext.RegisterConfigSettings(config))
                                   .GetCurrentClassLogger();

            try
            {
                var serviceProvider = BuildDi(config);
                using (serviceProvider as IDisposable)
                {
                    logger.Info("start program!!");
                    var program = serviceProvider.GetRequiredService<Program>();
                    program.DoAction();
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
            Console.ReadLine();
        }

        private static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
                .AddTransient<Program>()
                .AddLogging(builder => {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog(config);
                })
                .BuildServiceProvider();
        }

        public void DoAction()
        {
            TestSampleJob();
        }

        private void TestSampleJob()
        {
            var cs = new CancellationTokenSource();
            var smp = new SampleJobA(this._logger);
            var tsk = smp.ExecuteAsyncTask(cs.Token);
            _logger.LogInformation("Mainスレッドを2秒停止しますが、他スレッドには影響ありません");
            Thread.Sleep(2000);

            // _logger.LogInformation("キャンセルしますー");
            // cs.Cancel();

            _logger.LogInformation("失敗させますー");
            smp.SetFail = true;

            while (!(tsk.IsCanceled || tsk.IsCompleted || tsk.IsFaulted))
            {
                Thread.Sleep(1000);
            }
            _logger.LogInformation("Task Cancel");
            _logger.LogInformation($"IsCanceled: {tsk.IsCanceled}");
            _logger.LogInformation($"IsCompleted: {tsk.IsCompleted}");
            _logger.LogInformation($"IsFaulted: {tsk.IsFaulted}");
        }

    }
}
