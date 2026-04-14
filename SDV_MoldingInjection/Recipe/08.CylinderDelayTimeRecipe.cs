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
        public double ChamberOpenCloseCylinderMoveDelay
        {
            get => chamberOpenCloseCylinderMoveDelay;
            set => SetRecipe(ref chamberOpenCloseCylinderMoveDelay, value, nameof(ChamberOpenCloseCylinderMoveDelay));
        }
        
        [SingleRecipeDescription(Description = "Angle Valve Cylinder Move Delay", Unit = Unit.Second)]
        public double AngleValveCylinderMoveDelay
        {
            get => angleValveCylinderMoveDelay;
            set => SetRecipe(ref angleValveCylinderMoveDelay, value, nameof(AngleValveCylinderMoveDelay));
        }

        [SingleRecipeDescription(Description = "Bellow Cylinder Move Delay", Unit = Unit.Second)]
        public double BellowCylinderMoveDelay
        {
            get => bellowCylinderMoveDelay;
            set => SetRecipe(ref bellowCylinderMoveDelay, value, nameof(BellowCylinderMoveDelay));
        }

        [SingleRecipeDescription(Description = "NozzleClean Cylinder Move Delay", Unit = Unit.Second)]
        public double NozzleCleanCylinderMoveDelay_1
        {
            get => nozzleCleanCylinderMoveDelay_1;
            set => SetRecipe(ref nozzleCleanCylinderMoveDelay_1, value, nameof(NozzleCleanCylinderMoveDelay_1));
        }

        //[SingleRecipeDescription(Description = "NozzleClean 2 Cylinder Move Delay", Unit = Unit.Second)]
        //public double NozzleCleanCylinderMoveDelay_2
        //{
        //    get => nozzleCleanCylinderMoveDelay_2;
        //    set => SetRecipe(ref nozzleCleanCylinderMoveDelay_2, value, nameof(NozzleCleanCylinderMoveDelay_2));
        //}

        //[SingleRecipeDescription(Description = "NozzleClean 3 Cylinder Move Delay", Unit = Unit.Second)]
        //public double NozzleCleanCylinderMoveDelay_3
        //{
        //    get => nozzleCleanCylinderMoveDelay_3;
        //    set => SetRecipe(ref nozzleCleanCylinderMoveDelay_3, value, nameof(NozzleCleanCylinderMoveDelay_3));
        //}

        //[SingleRecipeDescription(Description = "NozzleClean 4 Cylinder Move Delay", Unit = Unit.Second)]
        //public double NozzleCleanCylinderMoveDelay_4
        //{
        //    get => nozzleCleanCylinderMoveDelay_4;
        //    set => SetRecipe(ref nozzleCleanCylinderMoveDelay_4, value, nameof(NozzleCleanCylinderMoveDelay_4));
        //}

        [SingleRecipeDescription(Description = "Piston 1 Cylinder Move Delay", Unit = Unit.Second)]
        public double PistonCylinderMoveDelay_1
        {
            get => pistonCylinderMoveDelay_1;
            set => SetRecipe(ref pistonCylinderMoveDelay_1, value, nameof(PistonCylinderMoveDelay_1));
        }

        [SingleRecipeDescription(Description = "Piston 2 Cylinder Move Delay", Unit = Unit.Second)]
        public double PistonCylinderMoveDelay_2
        {
            get => pistonCylinderMoveDelay_2;
            set => SetRecipe(ref pistonCylinderMoveDelay_2, value, nameof(PistonCylinderMoveDelay_2));
        }

        [SingleRecipeDescription(Description = "Piston 3 Cylinder Move Delay", Unit = Unit.Second)]
        public double PistonCylinderMoveDelay_3
        {
            get => pistonCylinderMoveDelay_3;
            set => SetRecipe(ref pistonCylinderMoveDelay_3, value, nameof(PistonCylinderMoveDelay_3));
        }

        [SingleRecipeDescription(Description = "Piston 4 Cylinder Move Delay", Unit = Unit.Second)]
        public double PistonCylinderMoveDelay_4
        {
            get => pistonCylinderMoveDelay_4;
            set => SetRecipe(ref pistonCylinderMoveDelay_4, value, nameof(PistonCylinderMoveDelay_4));
        }
    }
}
