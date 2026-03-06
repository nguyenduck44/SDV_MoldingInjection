using EQX.Core.Common;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines.CIM;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MaterialPortsViewModel : ViewModelBase
    {
        public List<MaterialPort> MaterialPorts => _cimCollection.MaterialPorts;

        private readonly CIMCollection _cimCollection;

        public MaterialPortsViewModel(CIMCollection cimCollection)
        {
            _cimCollection = cimCollection;
        }
    }
}
