
using CSCore.CoreAudioAPI;
using MuteAnyApp.Core.Enums;
using MuteAnyApp.Core.Helpers;
using MuteAnyApp.Core.Managers;
using MuteAnyApp.Core.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MuteAnyApp.Mutter
{
    internal class Program
    {
        static void Main(string[] args)
        {

            try
            {     
                var scenarioFile = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFileName);

                if (!File.Exists(scenarioFile))
                {
                    ServiceHelpers.WriteToLog("No scenario file");
                    return;
                }

                var sm = new ScenarioManager();

                var scenario = sm.GetCurrentScenario();
                
                if (scenario == null)
                {
                    ServiceHelpers.WriteToLog("Scenario was not deserealized");
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
                }

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
            catch (Exception ex)
            {
                ServiceHelpers.WriteToLog($"Fail when running scenario - {ex.Message}");
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