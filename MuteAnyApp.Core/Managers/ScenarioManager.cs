using MuteAnyApp.Core.Types;
using MuteAnyApp.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MuteAnyApp.Core.Managers
{
    public class ScenarioManager
    {
        #region Methods

        public bool SaveScenario(SoundChangeScenario scenario, out string error)
        {
            try
            {
                error = string.Empty;
                var mainScenariosDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

                if (!Directory.Exists(mainScenariosDir))
                {
                    Directory.CreateDirectory(mainScenariosDir);
                }

                var scenarioDirInfo = Directory.CreateDirectory(Path.Combine(mainScenariosDir, $"{scenario.Name}-({scenario.Id})"));

                var scenarioFile = Path.Combine(scenarioDirInfo.FullName, ServiceHelpers.ScenarioFileName);

                using (StreamWriter file = File.CreateText(scenarioFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, scenario);
                }

                if (!CopyExecutableAppToScenarioFolder(scenarioDirInfo.FullName, out error))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to save scenarios - &{ex.Message}";
                ServiceHelpers.WriteToLog(error);
                return false;
            }
        }

        public SoundChangeScenario? GetScenario(Guid scenarioGuid, out string error)
        {
            try
            {
                error = string.Empty;

                var mainScenariosDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

                if (!Directory.Exists(mainScenariosDir))
                {
                    Directory.CreateDirectory(mainScenariosDir);
                    return null;
                }

                var result = new SoundChangeScenario();

                foreach (var directory in Directory.GetDirectories(mainScenariosDir))
                {
                    if (string.IsNullOrEmpty(directory))
                    {
                        continue;
                    }

                    if (!directory.Contains(scenarioGuid.ToString()))
                    {
                        continue;
                    }

                    var scenarioFile = Path.Combine(directory, ServiceHelpers.ScenarioFileName);

                    if (!File.Exists(scenarioFile))
                    {
                        continue;
                    }

                    var jsonString = File.ReadAllText(scenarioFile);

                    result = JsonConvert.DeserializeObject<SoundChangeScenario>(jsonString);
                }

                return result;
            }
            catch (Exception ex)
            {
                error = $"Failed to get scenario {scenarioGuid} - {ex.Message}";
                ServiceHelpers.WriteToLog(error);
                return null;
            }
        }

        public IEnumerable<SoundChangeScenario> GetAllStoredScenarios(out string error)
        {
            try
            {
                error = string.Empty;

                var mainScenariosDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

                if (!Directory.Exists(mainScenariosDir))
                {
                    Directory.CreateDirectory(mainScenariosDir);
                    return new List<SoundChangeScenario>();
                }

                var result = new List<SoundChangeScenario>();

                foreach (var directory in Directory.GetDirectories(mainScenariosDir))
                {
                    var scenarioConfFile = Path.Combine(directory, ServiceHelpers.ScenarioFileName);
                    if (!File.Exists(scenarioConfFile))
                    {
                        continue;
                    }
                    var jsonString = File.ReadAllText(scenarioConfFile);

                    var scenario = JsonConvert.DeserializeObject<SoundChangeScenario>(jsonString);

                    if (scenario != null && directory.Contains(scenario.Id.ToString()))
                    {
                        result.Add(scenario);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                error = $"Failed to get scenarios - {ex.Message}";
                ServiceHelpers.WriteToLog(error);
                return new List<SoundChangeScenario>();
            }
        }

        public SoundChangeScenario? GetCurrentScenario()
        {
            try
            {
                var scenarioFileName = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFileName);
                if (!File.Exists(scenarioFileName))
                {
                    ServiceHelpers.WriteToLog($"File {scenarioFileName} not exists!");
                    return null;
                }

                var jsonString = File.ReadAllText(scenarioFileName);

                var result = JsonConvert.DeserializeObject<SoundChangeScenario>(jsonString);

                return result;
            }
            catch (Exception ex)
            {
                ServiceHelpers.WriteToLog($"Failed to get current scenario - {ex.Message}");
                return null;
            }
        }

        public bool RemoveScenario(Guid scenarioGuid, out string error)
        {
            try
            {
                error = string.Empty;
                var mainScenariosDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

                if (!Directory.Exists(mainScenariosDir))
                {
                    Directory.CreateDirectory(mainScenariosDir);
                    error = $"Directory {mainScenariosDir} not exists";
                    return false;
                }

                foreach (var directory in Directory.GetDirectories(mainScenariosDir))
                {
                    if (directory.Contains(scenarioGuid.ToString()))
                    {
                        Directory.Delete(directory, true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to remove scenario {scenarioGuid} - {ex.Message}";
                ServiceHelpers.WriteToLog(error);
                return false;
            }
        }

        private bool CopyExecutableAppToScenarioFolder(string folderPath, out string error)
        {
            try
            {
                error = string.Empty;
                var mainDir = ServiceHelpers.GetAssemblyDirectory();
                var appName = "SoundManagerConsole.exe";

                var appFilePath = Path.Combine(mainDir, appName);
                if (!File.Exists(appFilePath))
                {
                    // it's ok, maybe we dont have it in debug
                    // anyway it can be added manually
                    return true;
                }

                if (!Directory.Exists(folderPath))
                {
                    throw new Exception("Directory not exists! Wrong configuration!");
                }

                File.Copy(appFilePath, Path.Combine(folderPath, appName), true);

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to copy exec file to folder. {ex.Message}";
                ServiceHelpers.WriteToLog(error);
                return false;
            }
        }

        #endregion
    }
}
