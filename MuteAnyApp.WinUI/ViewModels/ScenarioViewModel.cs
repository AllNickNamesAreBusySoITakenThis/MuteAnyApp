using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSCore.CoreAudioAPI;
using MuteAnyApp.Core.Enums;
using MuteAnyApp.Core.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MuteAnyApp.WinUI.ViewModels
{
    public partial class ScenarioViewModel : ObservableObject
    {
        private Guid scenarioGuid;

        public ScenarioViewModel(SoundChangeScenario _scenario)
        {
            scenarioGuid = _scenario.Id == Guid.Empty ? Guid.NewGuid() : _scenario.Id;
            foreach (var item in _scenario.Actions)
            {
                SoundChangeActions.Add(item);
            }
            if(!string.IsNullOrEmpty(_scenario.Name))
            {
                ScenarioName = _scenario.Name;
            }
        }

        [ObservableProperty]
        bool? _dialogResult;

        //public bool? DialogResult
        //{
        //    get { return _dialogResult; }
        //    protected set
        //    {
        //        _dialogResult = value;
        //        OnPropertyChanged(nameof(DialogResult));
        //    }
        //}

        [ObservableProperty]
        string scenarioName = "Новый сценарий";

        public ObservableCollection<ProcessModel> DiscoveredProcess { get; set; } = new ObservableCollection<ProcessModel>();

        public ObservableCollection<SoundChangeAction> SoundChangeActions { get; set; } = new ObservableCollection<SoundChangeAction>();

        public SoundChangeScenario ResultScenario { get; set; }


        #region Commands


        #endregion

        #region Methods

        [RelayCommand]
        void Cancel()
        {
            DialogResult = false;
        }

        [RelayCommand]
        void Save()
        {
            ResultScenario = new SoundChangeScenario()
            {
                Id = scenarioGuid,
                Actions = SoundChangeActions.ToList(),
                Name = ScenarioName
            };

            DialogResult = true;
        }

        [RelayCommand]
        void AddAction()
        {
            var action = new SoundChangeAction()
            {
                Id = Guid.NewGuid(),
                ActionType = Core.Enums.ActionType.SetTo,
                Name = "Новое действие",
                ProcessName = string.Empty,
                SetToValue = 50f
            };
            SoundChangeActions.Add(action);
        }

        [RelayCommand]
        void AddProcessAction(ProcessModel process)
        {
            var action = new SoundChangeAction()
            {
                Id = Guid.NewGuid(),
                ActionType = Core.Enums.ActionType.SetTo,
                Name = $"Новое действие для {process.ProcessName}",
                ProcessName = process.ProcessName,
                SetToValue = 50f
            };
            SoundChangeActions.Add(action);
        }

        [RelayCommand]
        void RemoveAction(Guid actionsGuid)
        {
            var item = SoundChangeActions.FirstOrDefault(i=> i.Id == actionsGuid);
            if(item != null)
            {
                SoundChangeActions.Remove(item);
            }
        }

        [RelayCommand]
        async Task RefreshSoundProcesses()
        {
            await GetSoundProcess();
        }

        
        public async Task GetSoundProcess()
        {
            try
            {
                DiscoveredProcess.Clear();

                var procs = await Task.Run<List<ProcessModel>>(() =>
                {
                    var processes = new List<ProcessModel>();

                    foreach (var process in Process.GetProcesses())
                    {
                        processes.Add(new ProcessModel(process.Id, process.ProcessName));
                    }

                    var result = new List<ProcessModel>();

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
                                        result.Add(proc);
                                    }
                                }
                            }
                        }
                    }

                    return result;
                });

                foreach (var item in procs)
                {
                    DiscoveredProcess.Add(item);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
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

        #endregion
    }
}
