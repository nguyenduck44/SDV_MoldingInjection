using EQX.UI.MVVM;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMCollection
    {
        public List<MaterialPort> MaterialPorts { get; }

        public CIMCollection(IEnumerable<MaterialPort> materialPorts)
        {
            MaterialPorts = materialPorts.ToList();

            for (int i = 0; i < MaterialPorts.Count(); i++)
            {
                MaterialPorts[i].Id = i + 1;
                MaterialPorts[i].Name = $"Material Port Head {i + 1}";
                MaterialPorts[i].Type = "RESIN";
            }
            MaterialPorts.First().IsCurrentActived = true;
        }
    }
}
