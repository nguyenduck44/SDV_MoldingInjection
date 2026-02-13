using EQX.Core.Recipe;

namespace SDV_DemoEQ.Recipe
{
    public class StageRecipe : RecipeBase
    {
		private double yAxisLoadPosition;
        private double yAxisPlasmaStartPosition;
        private double yAxisPlasmaEndPosition;
        private double yAxisVisionAlignPosition;
        private double yAxisDispensingPosition;
        private double yAxisVisionInspectPosition;
        private double yAxisTransferPosition;
        private double plasmaSpeed;

        public double YAxisLoadPosition
		{
			get { return yAxisLoadPosition; }
			set { yAxisLoadPosition = value; }
		}

		public double YAxisPlasmaStartPosition
		{
			get { return yAxisPlasmaStartPosition; }
			set { yAxisPlasmaStartPosition = value; }
		}

		public double YAxisPlasmaEndPosition
		{
			get { return yAxisPlasmaEndPosition; }
			set { yAxisPlasmaEndPosition = value; }
		}

		public double YAxisVisionAlignPosition
		{
			get { return yAxisVisionAlignPosition; }
			set { yAxisVisionAlignPosition = value; }
		}

		public double YAxisDispensingPosition
		{
			get { return yAxisDispensingPosition; }
			set { yAxisDispensingPosition = value; }
		}

		public double YAxisVisionInspectPosition
		{
			get { return yAxisVisionInspectPosition; }
			set { yAxisVisionInspectPosition = value; }
		}

		public double YAxisTransferPositon
		{
			get { return yAxisTransferPosition; }
			set { yAxisTransferPosition = value; }
		}

		public double PlasmaSpeed
		{
			get { return plasmaSpeed; }
			set { plasmaSpeed = value; }
		}
	}
}
