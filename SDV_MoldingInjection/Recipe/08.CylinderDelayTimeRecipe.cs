using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class CylinderDelayTimeRecipe : RecipeBase
    {
        #region Privates
        private double chamberOpenCloseCylinderMoveDelay;
        private double angleValveCylinderMoveDelay;
        private double nozzleCleanCylinderMoveDelay_1;
        private double nozzleCleanCylinderMoveDelay_2;
        private double nozzleCleanCylinderMoveDelay_3;
        private double nozzleCleanCylinderMoveDelay_4;
        private double pistonCylinderMoveDelay_1;
        private double pistonCylinderMoveDelay_2;
        private double pistonCylinderMoveDelay_3;
        private double pistonCylinderMoveDelay_4;
        private double bellowCylinderMoveDelay;

        #endregion

        [SingleRecipeDescription(Description = "Chamber Open/Close Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(235)]
        public double ChamberOpenCloseCylinderMoveDelay
        {
            get => chamberOpenCloseCylinderMoveDelay;
            set => SetRecipe(ref chamberOpenCloseCylinderMoveDelay, value);
        }
        
        [SingleRecipeDescription(Description = "Angle Valve Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(236)]
        public double AngleValveCylinderMoveDelay
        {
            get => angleValveCylinderMoveDelay;
            set => SetRecipe(ref angleValveCylinderMoveDelay, value);
        }

        [SingleRecipeDescription(Description = "Bellow Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(237)]
        public double BellowCylinderMoveDelay
        {
            get => bellowCylinderMoveDelay;
            set => SetRecipe(ref bellowCylinderMoveDelay, value);
        }

        [SingleRecipeDescription(Description = "NozzleClean Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(238)]
        public double NozzleCleanCylinderMoveDelay_1
        {
            get => nozzleCleanCylinderMoveDelay_1;
            set => SetRecipe(ref nozzleCleanCylinderMoveDelay_1, value);
        }

        [SingleRecipeDescription(Description = "Piston 1 Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(239)]
        public double PistonCylinderMoveDelay_1
        {
            get => pistonCylinderMoveDelay_1;
            set => SetRecipe(ref pistonCylinderMoveDelay_1, value);
        }

        [SingleRecipeDescription(Description = "Piston 2 Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(240)]
        public double PistonCylinderMoveDelay_2
        {
            get => pistonCylinderMoveDelay_2;
            set => SetRecipe(ref pistonCylinderMoveDelay_2, value);
        }

        [SingleRecipeDescription(Description = "Piston 3 Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(241)]
        public double PistonCylinderMoveDelay_3
        {
            get => pistonCylinderMoveDelay_3;
            set => SetRecipe(ref pistonCylinderMoveDelay_3, value);
        }

        [SingleRecipeDescription(Description = "Piston 4 Cylinder Move Delay", Unit = Unit.Second)]
        [ParameterDescription(242)]
        public double PistonCylinderMoveDelay_4
        {
            get => pistonCylinderMoveDelay_4;
            set => SetRecipe(ref pistonCylinderMoveDelay_4, value);
        }
    }
}
