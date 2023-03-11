using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuteAnyApp.Core.Types;
using MuteAnyApp.Core.Helpers;
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
using MuteAnyApp.Core.Managers;

namespace MuteAnyApp.WinUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

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
        async Task DeleteScenario(Guid scenarioGuid)
        {
            try
            {
                var toRemove = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
                if (toRemove == null)
                {
                    return;
                }
                Scenarios.Remove(toRemove);
                await RemoveScenario(scenarioGuid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления сценария", MessageBoxButton.OK);
            }
        }

        //[RelayCommand]
        //void CreateBat(Guid scenarioGuid)
        //{
        //    try
        //    {
        //        var scenario = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
        //        if (scenario == null)
        //        {
        //            return;
        //        }

        //        var vbsText = $"""
        //            Set WshShell = CreateObject("WScript.Shell" ) 
        //            strPath = Wscript.ScriptFullName
        //            Set objFSO = CreateObject("Scripting.FileSystemObject")
        //            Set objFile = objFSO.GetFile(strPath)
        //            strFolder = objFSO.GetParentFolderName(objFile) 
        //            strFolder = objFSO.GetParentFolderName(strFolder) 
        //            strPath = strFolder & "\SoundManagerConsole.exe {scenarioGuid}"
        //            WshShell.Run strPath, 0 
        //            Set WshShell = Nothing  
        //            """;

        //        // var batText = $"start /d \"{Directory.GetCurrentDirectory()}\" SoundManagerConsole.exe {scenarioGuid}";

        //        var batDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

        //        if (!Directory.Exists(batDir))
        //        {
        //            Directory.CreateDirectory(batDir);
        //        }

        //        File.WriteAllText(Path.Combine(batDir, $"Scenario-{scenario.Name}.vbs"), vbsText);

        //        MessageBox.Show(".bat created!", "Response", MessageBoxButton.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceHelpers.WriteToLog($"Ошибка создания исполняемого файла : {ex.Message}");
        //        MessageBox.Show(ex.Message, "Ошибка создания исполняемого файла", MessageBoxButton.OK);
        //    }
        //}

        //[RelayCommand]
        //void CreateLnk(Guid scenarioGuid)
        //{
        //    try
        //    {
        //        var scenario = Scenarios.FirstOrDefault(s => s.Id == scenarioGuid);
        //        if (scenario == null)
        //        {
        //            return;
        //        }
        //        IShellLink link = (IShellLink)new ShellLink();

        //        var scenarioDataDir = Path.Combine(ServiceHelpers.GetAssemblyDirectory(), ServiceHelpers.ScenarioFolderName);

        //        // setup shortcut information
        //        link.SetDescription("Link to run scenario");
        //        link.SetPath(Path.Combine(ServiceHelpers.GetAssemblyDirectory(), "SoundManagerConsole.exe"));
        //        link.SetArguments(scenarioGuid.ToString());

        //        // save it
        //        IPersistFile file = (IPersistFile)link;
        //        file.Save(Path.Combine(scenarioDataDir, $"Run_{scenario.Name}.lnk"), false);

        //        MessageBox.Show("Ярлык для запуска был успешно создан", "Успех", MessageBoxButton.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceHelpers.WriteToLog($"Ошибка создания ярлыка исполняемого файла : {ex.Message}");
        //        MessageBox.Show(ex.Message, "Ошибка создания ярлыка исполняемого файла", MessageBoxButton.OK);
        //    }
        //}

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
                    var sm = new ScenarioManager();
                    var result = sm.GetAllStoredScenarios(out var error);
                    if (result == null || !string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                    return result.ToList();
                });

                foreach (var item in storedScenarios)
                {
                    Scenarios.Add(item);
                }
            }
            catch (Exception ex)
            {
                // ServiceHelpers.WriteToLog($"Ошибка загрузки сценариев : {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка загрузки сценариев", MessageBoxButton.OK);
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task SaveScenarios(IEnumerable<SoundChangeScenario> scenariosToStore)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (var scenario in scenariosToStore)
                    {
                        var sm = new ScenarioManager();
                        if (!sm.SaveScenario(scenario, out var error))
                        {
                            throw new Exception(error);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // ServiceHelpers.WriteToLog($"Ошибка сохранения сценариев : {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка сохранения сценариев", MessageBoxButton.OK);
            }
        }

        async Task RemoveScenario(Guid scenarioGuid)
        {
            try
            {
                await Task.Run(() =>
                {
                    var sm = new ScenarioManager();
                    if (!sm.RemoveScenario(scenarioGuid, out var error))
                    {
                        throw new Exception(error);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления сценария", MessageBoxButton.OK);
            }
        }
        #endregion
    }
}
