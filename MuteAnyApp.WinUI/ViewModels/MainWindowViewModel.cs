using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuteAnyApp.Core.Types;
using MuteAnyApp.WinUI.Types;
using MuteAnyApp.WinUI.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MuteAnyApp.WinUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        const string ScenarioFolderName = "Scenarios";
        const string ScenarioFileName = "CreatedScenarios.json";
        const string LogFolderName = "Logs";
        const string LogFileName = "Logs.txt";

        public ObservableCollection<SoundChangeScenario> Scenarios { get; set; } = new ObservableCollection<SoundChangeScenario>();

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }


        #region Commands

        [RelayCommand]
        async Task CreateScenario()
        {
            var scWindow = new ScenarioWindow(new ScenarioViewModel(new SoundChangeScenario()));
            var result = scWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var context = (scWindow.DataContext as ScenarioViewModel);
                if (context != null)
                {
                    var scenario = context.ResultScenario;
                    if (scenario != null)
                    {
                        Scenarios.Add(scenario);
                        await SaveScenarios(Scenarios);
                    }
                }
            }
        }

        [RelayCommand]
        async Task EditScenario(Guid scenarioGuid)
        {
            var scenario = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
            if (scenario == null)
            {
                return;
            }

            var scWindow = new ScenarioWindow(new ScenarioViewModel(scenario));
            var result = scWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var context = (scWindow.DataContext as ScenarioViewModel);
                if (context != null)
                {
                    var updScenario = context.ResultScenario;
                    if (updScenario != null)
                    {
                        Scenarios.Remove(scenario);
                        await Task.Delay(100);
                        Scenarios.Add(updScenario);
                        await SaveScenarios(Scenarios);
                    }
                }
            }
        }

        [RelayCommand]
        async Task RemoveScenario(Guid scenarioGuid)
        {
            try
            {
                var toRemove = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
                if (toRemove == null)
                {
                    return;
                }
                Scenarios.Remove(toRemove);
                await SaveScenarios(Scenarios);
            }
            catch (Exception ex)
            {
                WriteToLog($"Ошибка удаления сценария : {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка удаления сценария", MessageBoxButton.OK);
            }
        }

        [RelayCommand]
        void CreateBat(Guid scenarioGuid)
        {
            try
            {
                var scenario = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
                if (scenario == null)
                {
                    return;
                }

                var batText = $"start /d \"{Directory.GetCurrentDirectory()}\" SoundManagerConsole.exe {scenarioGuid}";

                var batDir = Path.Combine(Directory.GetCurrentDirectory(), "ScenariosBats");

                if (!Directory.Exists(batDir))
                {
                    Directory.CreateDirectory(batDir);
                }

                File.WriteAllText(Path.Combine(batDir,$"Scenario-{scenario.Name}.bat"), batText);

                MessageBox.Show(".bat created!", "Response", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                WriteToLog($"Ошибка создания исполняемого файла : {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка создания исполняемого файла", MessageBoxButton.OK);
            }
        }

        #endregion

        #region Methods

        public async Task LoadScenariosCommand(object param)
        {
            try
            {
                IsLoading = true;

                Scenarios.Clear();

                await Task.Delay(200);

                var storedScenarios = await Task.Run<List<SoundChangeScenario>>(() =>
                {
                    var logsDir = Path.Combine(Directory.GetCurrentDirectory(), LogFolderName);

                    if (!Directory.Exists(logsDir))
                    {
                        Directory.CreateDirectory(logsDir);
                    }

                    var scenarioDir = Path.Combine(Directory.GetCurrentDirectory(), ScenarioFolderName);

                    if (!Directory.Exists(scenarioDir))
                    {
                        Directory.CreateDirectory(scenarioDir);
                        return new List<SoundChangeScenario>(); ;
                    }

                    var scenarioFile = Path.Combine(scenarioDir, ScenarioFileName);

                    if (!File.Exists(scenarioFile))
                    {
                        File.Create(scenarioFile);
                        return new List<SoundChangeScenario>();
                    }

                    var jsonString = File.ReadAllText(scenarioFile);

                    var storedScenarios = JsonConvert.DeserializeObject<IEnumerable<SoundChangeScenario>>(jsonString);
                    if (storedScenarios == null)
                    {
                        return new List<SoundChangeScenario>();
                    }

                    var result = new List<SoundChangeScenario>();

                    foreach (var scenario in storedScenarios)
                    {
                        result.Add(scenario);
                    }

                    return result;
                });

                foreach (var item in storedScenarios)
                {
                    Scenarios.Add(item);
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"Ошибка загрузки сценариев : {ex.Message}");
                MessageBox.Show( ex.Message, "Ошибка загрузки сценариев", MessageBoxButton.OK);
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task SaveScenarios(IEnumerable<SoundChangeScenario> scenarios)
        {
            try
            {
                await Task.Run(() =>
                {
                    var logsDir = Path.Combine(Directory.GetCurrentDirectory(), LogFolderName);

                    if (!Directory.Exists(logsDir))
                    {
                        Directory.CreateDirectory(logsDir);
                    }

                    var scenarioDir = Path.Combine(Directory.GetCurrentDirectory(), ScenarioFolderName);

                    if (!Directory.Exists(scenarioDir))
                    {
                        Directory.CreateDirectory(scenarioDir);
                    }

                    var scenarioFile = Path.Combine(scenarioDir, ScenarioFileName);

                    using (StreamWriter file = File.CreateText(scenarioFile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, scenarios);
                    }
                });
            }
            catch (Exception ex)
            {
                WriteToLog($"Ошибка сохранения сценариев : {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка сохранения сценариев", MessageBoxButton.OK);
            }
        }

        void WriteToLog(string message)
        {
            try
            {
                var logsDir = Path.Combine(Directory.GetCurrentDirectory(), LogFolderName);

                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                var logFileName = Path.Combine(logsDir, LogFileName);

                if (File.Exists(logFileName))
                {
                    var content = File.ReadAllLines(logFileName).ToList();
                    if (content.Count > 200)
                    {
                        content = content.Take(199).ToList();
                    }
                    content.Insert(0, $"{DateTime.Now} - {message}");
                    File.WriteAllLines(logFileName, content);
                }
                else
                {
                    File.WriteAllLines(logFileName, new string[] { $"{DateTime.Now} - {message}" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка записи лога", MessageBoxButton.OK);
            }
        }

        #endregion        
    }
}
