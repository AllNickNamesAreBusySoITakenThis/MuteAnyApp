using MuteAnyApp.Core.Abstract;
using MuteAnyApp.Core.Types;

namespace MuteAppService
{
    public class MuteAppWorker : BackgroundService
    {
        private readonly ILogger<MuteAppWorker> m_logger;
        private readonly IScenariosManager m_scenariosManager;
        private List<SoundChangeScenario> scenariosCache = new List<SoundChangeScenario>();
        private bool needRefreshScenarios = true;

        public MuteAppWorker(ILogger<MuteAppWorker> logger)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //m_scenariosManager = scenariosManager ?? throw new ArgumentNullException(nameof(scenariosManager));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (m_logger.IsEnabled(LogLevel.Information))
            //    {
            //        m_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc, m_logger);
            Console.WriteLine("Start hoooks");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            //InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
            //Console.WriteLine("End hoooks");
        }

        private async Task ProcessScenarios()
        {
            if (needRefreshScenarios)
            {
                scenariosCache = new List<SoundChangeScenario>(m_scenariosManager.GetAllScenarios() ?? new List<SoundChangeScenario>());
            }

        }
    }
}
