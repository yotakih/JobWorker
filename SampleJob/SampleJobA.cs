using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace SampleJob
{
    public class SampleJobA
    {
        public bool SetFail = false;

        private readonly ILogger _logger;

        public SampleJobA(ILogger logger)
        {
            this._logger = logger;
        }

        public async Task ExecuteAsyncTask(CancellationToken _cancelToken)
        {
            try
            {
                await Task.Run(()=>{
                    ExecuteJob(_cancelToken);
                }, _cancelToken);
            }
            catch(OperationCanceledException e)
            {
                _logger.LogError($"処理がキャンセルされました");
            }
            catch(Exception e)
            {
                _logger.LogError($"{e.Message}");
                throw;
            }
            finally
            {
                //データベースなどのセッションを閉じる
                _logger.LogInformation("SampleTaskA exec finally");
            }
        }

        private void ExecuteJob(CancellationToken _cancelToken)
        {
            // hevy job
            for(var i=0;i<5;i++)
            {
                //処理がキャンセルされた場合、キャンセル例外をスロー
                _cancelToken.ThrowIfCancellationRequested();
                if (this.SetFail) throw new Exception("Set Fail Execption");
                _logger.LogInformation("SampleJob Execute!!");
                Thread.Sleep(1000);
            }
        }

    }
}
