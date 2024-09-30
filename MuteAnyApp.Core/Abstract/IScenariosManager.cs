using MuteAnyApp.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Abstract
{
    public interface IScenariosManager
    {
        bool CreateScenario(SoundChangeScenario scenario);

        bool UpdateScenario(SoundChangeScenario scenario);

        bool DeleteScenario(Guid scenarioId);

        IEnumerable<SoundChangeScenario> GetAllScenarios();

        bool ClearScenarios();

        bool UpdateScenarioProperty<T>(Guid scenarioId, Expression<Func<SoundChangeScenario, T>> expression, T value);

        bool ValidateScenario(SoundChangeScenario scenario, out string error);
    }
}
