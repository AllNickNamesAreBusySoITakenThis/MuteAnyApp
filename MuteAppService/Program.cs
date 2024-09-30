using MuteAnyApp.Core.Abstract;
using MuteAnyApp.Core.Managers;
using MuteAppService;

var builder = Host.CreateApplicationBuilder(args);
//LoggerFactory
//builder.Services.AddLogging();
//builder.Services.AddTransient<IScenariosManager, ScenariosManagerJson>();
builder.Services.AddHostedService<MuteAppWorker>();

var host = builder.Build();
host.Run();
