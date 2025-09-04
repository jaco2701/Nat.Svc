using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace Nat.Svc
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> mioLogger;
        private NatSetting mioNatSetting;

        public Worker(ILogger<Worker> vioLogger, IOptions<NatSetting> vioOptionsNatSetting)
        {
            mioLogger = vioLogger;
            mioNatSetting = vioOptionsNatSetting.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (mioLogger.IsEnabled(LogLevel.Information))
                {
                    mioLogger.LogInformation("{time}: Ejecucion Servicio ", DateTimeOffset.Now);
                }
                HttpClientHandler lioHttpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                HttpClient lioHttpClient = new HttpClient(lioHttpClientHandler);
                lioHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "NatSvc",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(mioNatSetting.ivstrCreds))
                );
                try
                {
                    HttpResponseMessage lioResponse = await lioHttpClient.GetAsync(mioNatSetting.ivstrUrl);
                    if (!lioResponse.IsSuccessStatusCode)
                        mioLogger.LogInformation("{time}: Error Servicio --> {error}", DateTimeOffset.Now, lioResponse.RequestMessage);
                }
                catch (Exception lioE)
                {
                    mioLogger.LogError("{time}: Error Servicio --> {error}", DateTimeOffset.Now, lioE);
                }
                finally
                {
                    lioHttpClient.Dispose();
                    await Task.Delay((mioNatSetting.ivnumDelaySecs ?? 60) * 1000, stoppingToken);
                }
            }
        }
    }
}
