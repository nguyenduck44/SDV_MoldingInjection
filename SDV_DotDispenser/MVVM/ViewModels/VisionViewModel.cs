using EQX.Core.Common;
using EQX.Core.Vision.Algorithms;
using EQX.Vision.Algorithms;

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class VisionViewModel : ViewModelBase
    {
        private readonly IVisionFlowRepository _visionFlowRepository;
        private readonly IVisionToolRepository _visionToolRepository;

        public VisionViewModel(IVisionFlowRepository visionFlowRepository,
            IVisionToolRepository visionToolRepository)
        {
            _visionFlowRepository = visionFlowRepository;
            _visionToolRepository = visionToolRepository;
        }

        public List<IVisionTool> VisionTools => _visionToolRepository.GetAll().ToList();

        public IVisionFlow AlignFlow => _visionFlowRepository.GetAll().FirstOrDefault(vf => vf.Name == "AlignFlow") ?? new VisionFlow("AlignFlow");
    }
}
