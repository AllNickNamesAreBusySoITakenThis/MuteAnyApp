using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MuteAnyApp.Core.Abstract;
using MuteAnyApp.Core.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Managers
{
    public class ScenariosManagerJson : IScenariosManager
    {
        private readonly string m_filePath;
        private readonly ILogger m_logger;
        private readonly IConfiguration m_configuration;

        public ScenariosManagerJson(ILogger logger, IConfiguration configuration)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_filePath = configuration.GetSection("ApplicationSettings.FilePath").Value!;
        }

        public bool ClearScenarios()
        {
            try
            {
                if (!CheckFile())
                {
                    return false;
                }

                File.WriteAllText(m_filePath, string.Empty);
                m_logger.LogWarning("Scenarios file was cleared");
                return true;
            }
            catch (Exception ex)
            {
                m_logger.LogError($"Failed to clear scenarios file: {ex.Message}");
                return false;
            }
        }

        public bool CreateScenario(SoundChangeScenario scenario)
        {
            try
            {
                if (scenario == null)
                {
                    m_logger.LogWarning("Empty scenario added for creation");
                    return false;
                }
                if (!CheckFile())
                {
                    m_logger.LogWarning("File was not pass the check. Creation is not possible");
                    return false;
                }
                if (!ValidateScenario(scenario, out var error))
                {
                    m_logger.LogWarning($"Scenario is not valid. {error}");
                    return false;
                }
                var scenarios = GetAllScenarios().ToList();
                scenarios.Add(scenario);
                using (var fileWriter = File.CreateText(m_filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(fileWriter, scenarios);
                }
                return true;
            }
            catch (Exception ex)
            {
                m_logger.LogError($"Failed to create scenario. {ex.Message}");
                return false;
            }
        }

        public bool DeleteScenario(Guid scenarioId)
        {
            try
            {
                if (scenarioId == Guid.Empty)
                {
                    m_logger.LogWarning("Empty scenarioId");
                    return false;
                }
                if (!CheckFile())
                {
                    m_logger.LogWarning("File was not pass the check. Deleting is not possible");
                    return false;
                }
                var scenarios = GetAllScenarios().Where(s => s.Id != scenarioId);
                using (var fileWriter = File.CreateText(m_filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(fileWriter, scenarios);
                }
                return true;
            }
            catch (Exception ex)
            {
                m_logger.LogError($"Failed to remove scenario. {ex.Message}");
                return false;
            }
        }

        public IEnumerable<SoundChangeScenario> GetAllScenarios()
        {
            try
            {
                if (!CheckFile())
                {
                    m_logger.LogWarning("File was not pass the check. Can not get scenarios.");
                    return null;
                }
                var scenarios = JsonConvert.DeserializeObject<List<SoundChangeScenario>>(m_filePath);
                return scenarios;
            }
            catch (Exception ex)
            {
                m_logger.LogError($"Failed to get all scenarios. {ex.Message}");
                return null;
            }
        }

        public bool UpdateScenario(SoundChangeScenario scenario)
        {
            try
            {
                if (scenario == null)
                {
                    m_logger.LogWarning("Empty scenario added for update");
                    return false;
                }
                if (!CheckFile())
                {
                    m_logger.LogWarning("File was not pass the check. Can not update scenario.");
                    return false;
                }
                if (!ValidateScenario(scenario, out var error))
                {
                    m_logger.LogWarning($"Scenario is not valid. {error}");
                    return false;
                }
                var scenarios = GetAllScenarios().Where(s => s.Id != scenario.Id).ToList();
                scenarios.Add(scenario);
                using (var fileWriter = File.CreateText(m_filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(fileWriter, scenarios);
                }
                return true;
            }
            catch (Exception ex)
            {
                m_logger.LogError($"Failed to update scenario. {ex.Message}");
                return false;
            }
        }

        public bool UpdateScenarioProperty<T>(Guid scenarioId, Expression<Func<SoundChangeScenario, T>> expression, T value)
        {
            m_logger.LogError("This method not needed for JSON storage");
            return false;
        }

        public bool ValidateScenario(SoundChangeScenario scenario, out string error)
        {
            error = string.Empty;
            if (GetAllScenarios().Any(s => s.Name == scenario.Name && s.Id != scenario.Id))
            {
                error = $"Scenario with name {scenario.Name} is already exists.";
                return false;
            }

            return true;
        }

        private bool CheckFile()
        {
            if (string.IsNullOrEmpty(m_filePath))
            {
                m_logger.LogError("Empty scenarios file name");
                return false;
            }

            if (!File.Exists(m_filePath))
            {
                File.Create(m_filePath);
            }

            return true;
        }


    }
}
