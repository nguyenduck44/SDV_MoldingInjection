using EQX.Core.Common;
using EQX.Core.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public abstract class MaintenanceTeachingPositionsBase : ViewModelBase
    {
        protected MultiPointPosition CreateGroup(string name, params PositionPoint[] points)
        {
            return new MultiPointPosition
            {
                Name = name,
                Points = new ObservableCollection<PositionPoint>(points),
            };
        }
    }
}
