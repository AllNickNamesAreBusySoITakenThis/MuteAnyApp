
using CSCore.CoreAudioAPI;
using MuteAnyApp.Core.Enums;
using MuteAnyApp.Core.Types;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace MuteAnyApp.Mutter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string ScenarioFolderName = "Scenarios";
            const string ScenarioFileName = "CreatedScenarios.json";
            const string LogFolderName = "Logs";

            var logsDir = Path.Combine(Directory.GetCurrentDirectory(), LogFolderName);

            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            var scenarioDir = Path.Combine(Directory.GetCurrentDirectory(), ScenarioFolderName);

            if (!Directory.Exists(scenarioDir))
            {
                Directory.CreateDirectory(scenarioDir);
                return;
            }

            var scenarioFile = Path.Combine(scenarioDir, ScenarioFileName);

            if (!File.Exists(scenarioFile))
            {
                File.Create(scenarioFile);
                return;
            }

            if (args.Length == 0)
            {
                return;
            }

            if (!Guid.TryParse(args[0], out var scenarioGuid))
            {
                return;
            }

            var jsonString = File.ReadAllText(scenarioFile);

            var scenario = JsonConvert.DeserializeObject<IEnumerable<SoundChangeScenario>>(jsonString).FirstOrDefault(s => s.Id.Equals(scenarioGuid));

            if (scenario == null)
            {
                return;
            }

            var processes = new List<ProcessModel>();

            foreach (var process in Process.GetProcesses())
            {
                var action = scenario.Actions.FirstOrDefault(a => a.ProcessName == process.ProcessName);

                if (action == null)
                {
                    continue;
                }

                processes.Add(new ProcessModel(process.Id, process.ProcessName, action));
                Console.WriteLine($"{process.ProcessName} - {process.Id}");
            }

            Console.WriteLine();
            Console.WriteLine("Collecting processes done!");

            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>())
                        {
                            var proc = processes.FirstOrDefault(p => p.ProcessId == sessionControl.ProcessID);
                            if (proc != null)
                            {
                                switch (proc.ProcessAction.ActionType)
                                {
                                    case ActionType.Mute:
                                        simpleVolume.IsMuted = true;
                                        break;
                                    case ActionType.Unmute:
                                        simpleVolume.IsMuted = false;
                                        break;
                                    case ActionType.SetTo:
                                        var value = (float)(proc.ProcessAction.SetToValue / 100);
                                        simpleVolume.MasterVolume = value;
                                        break;
                                }
                            }


                        }
                    }
                }
            }
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    Debug.WriteLine("DefaultDevice: " + device.FriendlyName);
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }
    }
}