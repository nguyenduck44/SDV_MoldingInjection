using EQX.Core.Recipe;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Recipe
{
    public class MotionSpeedRecipe : RecipeBase
    {
        #region private
        private double xAxisVelocity;
        private double xAxisAccel;
        private double xAxisDeccel;

        private double stageYAxisVelocity;
        private double stageYAxisAccel;
        private double stageYAxisDeccel;

        private double z1AxisVelocity;
        private double z1AxisAccel;
        private double z1AxisDeccel;

        private double z2AxisVelocity;
        private double z2AxisAccel;
        private double z2AxisDeccel;

        private double z3AxisVelocity;
        private double z3AxisAccel;
        private double z3AxisDeccel;

        private double z4AxisVelocity;
        private double z4AxisAccel;
        private double z4AxisDeccel;

        private double p1AxisVelocity;
        private double p1AxisAccel;
        private double p1AxisDeccel;

        private double p2AxisVelocity;
        private double p2AxisAccel;
        private double p2AxisDeccel;

        private double p3AxisVelocity;
        private double p3AxisAccel;
        private double p3AxisDeccel;

        private double p4AxisVelocity;
        private double p4AxisAccel;
        private double p4AxisDeccel;

        private double g1AxisVelocity;
        private double g1AxisAccel;
        private double g1AxisDeccel;

        private double g2AxisVelocity;
        private double g2AxisAccel;
        private double g2AxisDeccel;

        private double g3AxisVelocity;
        private double g3AxisAccel;
        private double g3AxisDeccel;

        private double g4AxisVelocity;
        private double g4AxisAccel;
        private double g4AxisDeccel;
        #endregion

        [JsonIgnore]
        public Action<double> XAxisVelocityChanged;
        [JsonIgnore]
        public Action<double> XAxisAccelChanged;
        [JsonIgnore]
        public Action<double> XAxisDeccelChanged;

        [JsonIgnore]
        public Action<double> StageYAxisVelocityChanged;
        [JsonIgnore]
        public Action<double> StageYAxisAccelChanged;
        [JsonIgnore]
        public Action<double> StageYAxisDeccelChanged;

        [JsonIgnore]
        public Action<double> Z1AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> Z1AxisAccelChanged;
        [JsonIgnore]
        public Action<double> Z1AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> Z2AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> Z2AxisAccelChanged;
        [JsonIgnore]
        public Action<double> Z2AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> Z3AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> Z3AxisAccelChanged;
        [JsonIgnore]
        public Action<double> Z3AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> Z4AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> Z4AxisAccelChanged;
        [JsonIgnore]
        public Action<double> Z4AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> P1AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> P1AxisAccelChanged;
        [JsonIgnore]
        public Action<double> P1AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> P2AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> P2AxisAccelChanged;
        [JsonIgnore]
        public Action<double> P2AxisDeccelChanged;


        [JsonIgnore]
        public Action<double> P3AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> P3AxisAccelChanged;
        [JsonIgnore]
        public Action<double> P3AxisDeccelChanged;


        [JsonIgnore]
        public Action<double> P4AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> P4AxisAccelChanged;
        [JsonIgnore]
        public Action<double> P4AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> G1AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> G1AxisAccelChanged;
        [JsonIgnore]
        public Action<double> G1AxisDeccelChanged;

        [JsonIgnore]
        public Action<double> G2AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> G2AxisAccelChanged;
        [JsonIgnore]
        public Action<double> G2AxisDeccelChanged;


        [JsonIgnore]
        public Action<double> G3AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> G3AxisAccelChanged;
        [JsonIgnore]
        public Action<double> G3AxisDeccelChanged;


        [JsonIgnore]
        public Action<double> G4AxisVelocityChanged;
        [JsonIgnore]
        public Action<double> G4AxisAccelChanged;
        [JsonIgnore]
        public Action<double> G4AxisDeccelChanged;

        [ParameterDescription(247)]
        public double XAxisVelocity
        {
            get { return xAxisVelocity; }
            set
            {
                if (xAxisVelocity == value) return;
                SetRecipe(ref xAxisVelocity, value);
                XAxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(248)]
        public double XAxisAccel
        {
            get { return xAxisAccel; }
            set
            {
                if (xAxisAccel == value) return;
                SetRecipe(ref xAxisAccel, value);
                XAxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(249)]
        public double XAxisDeccel
        {
            get { return xAxisDeccel; }
            set
            {
                if (xAxisDeccel == value) return;
                SetRecipe(ref xAxisDeccel, value);
                XAxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(250)]
        public double StageYAxisVelocity
        {
            get { return stageYAxisVelocity; }
            set
            {
                if (stageYAxisVelocity == value) return;
                SetRecipe(ref stageYAxisVelocity, value);
                StageYAxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(251)]
        public double StageYAxisAccel
        {
            get { return stageYAxisAccel; }
            set
            {
                if (stageYAxisAccel == value) return;
                SetRecipe(ref stageYAxisAccel, value);
                StageYAxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(252)]
        public double StageYAxisDeccel
        {
            get { return stageYAxisDeccel; }
            set
            {
                if (stageYAxisDeccel == value) return;
                SetRecipe(ref stageYAxisDeccel, value);
                StageYAxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(253)]
        public double Z1AxisVelocity
        {
            get { return z1AxisVelocity; }
            set
            {
                if (z1AxisVelocity == value) return;
                SetRecipe(ref z1AxisVelocity, value);
                Z1AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(254)]
        public double Z1AxisAccel
        {
            get { return z1AxisAccel; }
            set
            {
                if (z1AxisAccel == value) return;
                SetRecipe(ref z1AxisAccel, value);
                Z1AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(255)]
        public double Z1AxisDeccel
        {
            get { return z1AxisDeccel; }
            set
            {
                if (z1AxisDeccel == value) return;
                SetRecipe(ref z1AxisDeccel, value);
                Z1AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(256)]
        public double Z2AxisVelocity
        {
            get { return z2AxisVelocity; }
            set
            {
                if (z2AxisVelocity == value) return;
                SetRecipe(ref z2AxisVelocity, value);
                Z2AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(257)]
        public double Z2AxisAccel
        {
            get { return z2AxisAccel; }
            set
            {
                if (z2AxisAccel == value) return;
                SetRecipe(ref z2AxisAccel, value);
                Z2AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(258)]
        public double Z2AxisDeccel
        {
            get { return z2AxisDeccel; }
            set
            {
                if (z2AxisDeccel == value) return;
                SetRecipe(ref z2AxisDeccel, value);
                Z2AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(259)]
        public double Z3AxisVelocity
        {
            get { return z3AxisVelocity; }
            set
            {
                if (z3AxisVelocity == value) return;
                SetRecipe(ref z3AxisVelocity, value);
                Z3AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(260)]
        public double Z3AxisAccel
        {
            get { return z3AxisAccel; }
            set
            {
                if (z3AxisAccel == value) return;
                SetRecipe(ref z3AxisAccel, value);
                Z3AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(261)]
        public double Z3AxisDeccel
        {
            get { return z3AxisDeccel; }
            set
            {
                if (z3AxisDeccel == value) return;
                SetRecipe(ref z3AxisDeccel, value);
                Z3AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(262)]
        public double Z4AxisVelocity
        {
            get { return z4AxisVelocity; }
            set
            {
                if (z4AxisVelocity == value) return;
                SetRecipe(ref z4AxisVelocity, value);
                Z4AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(263)]
        public double Z4AxisAccel
        {
            get { return z4AxisAccel; }
            set
            {
                if (z4AxisAccel == value) return;
                SetRecipe(ref z4AxisAccel, value);
                Z4AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(264)]
        public double Z4AxisDeccel
        {
            get { return z4AxisDeccel; }
            set
            {
                if (z4AxisDeccel == value) return;
                SetRecipe(ref z4AxisDeccel, value);
                Z4AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(265)]
        public double P1AxisVelocity
        {
            get { return p1AxisVelocity; }
            set
            {
                if (p1AxisVelocity == value) return;
                SetRecipe(ref p1AxisVelocity, value);
                P1AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(266)]
        public double P1AxisAccel
        {
            get { return p1AxisAccel; }
            set
            {
                if (p1AxisAccel == value) return;
                SetRecipe(ref p1AxisAccel, value);
                P1AxisAccelChanged?.Invoke(value);
            }
        }
        
        [ParameterDescription(267)]
        public double P1AxisDeccel
        {
            get { return p1AxisDeccel; }
            set
            {
                if (p1AxisDeccel == value) return;
                SetRecipe(ref p1AxisDeccel, value);
                P1AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(268)]
        public double P2AxisVelocity
        {
            get { return p2AxisVelocity; }
            set
            {
                if (p2AxisVelocity == value) return;
                SetRecipe(ref p2AxisVelocity, value);
                P2AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(269)]
        public double P2AxisAccel
        {
            get { return p2AxisAccel; }
            set
            {
                if (p2AxisAccel == value) return;
                SetRecipe(ref p2AxisAccel, value);
                P2AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(270)]
        public double P2AxisDeccel
        {
            get { return p2AxisDeccel; }
            set
            {
                if (p2AxisDeccel == value) return;
                SetRecipe(ref p2AxisDeccel, value);
                P2AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(271)]
        public double P3AxisVelocity
        {
            get { return p3AxisVelocity; }
            set
            {
                if (p3AxisVelocity == value) return;
                SetRecipe(ref p3AxisVelocity, value);
                P3AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(272)]
        public double P3AxisAccel
        {
            get { return p3AxisAccel; }
            set
            {
                if (p3AxisAccel == value) return;
                SetRecipe(ref p3AxisAccel, value);
                P3AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(273)]
        public double P3AxisDeccel
        {
            get { return p3AxisDeccel; }
            set
            {
                if (p3AxisDeccel == value) return;
                SetRecipe(ref p3AxisDeccel, value);
                P3AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(274)]
        public double P4AxisVelocity
        {
            get { return p4AxisVelocity; }
            set
            {
                if (p4AxisVelocity == value) return;
                SetRecipe(ref p4AxisVelocity, value);
                P4AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(275)]
        public double P4AxisAccel
        {
            get { return p4AxisAccel; }
            set
            {
                if (p4AxisAccel == value) return;
                SetRecipe(ref p4AxisAccel, value);
                P4AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(276)]
        public double P4AxisDeccel
        {
            get { return p4AxisDeccel; }
            set
            {
                if (p4AxisDeccel == value) return;
                SetRecipe(ref p4AxisDeccel, value);
                P4AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(277)]
        public double G1AxisVelocity
        {
            get { return g1AxisVelocity; }
            set
            {
                if (g1AxisVelocity == value) return;
                SetRecipe(ref g1AxisVelocity, value);
                G1AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(278)]
        public double G1AxisAccel
        {
            get { return g1AxisAccel; }
            set
            {
                if (g1AxisAccel == value) return;
                SetRecipe(ref g1AxisAccel, value);
                G1AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(279)]
        public double G1AxisDeccel
        {
            get { return g1AxisDeccel; }
            set
            {
                if (g1AxisDeccel == value) return;
                SetRecipe(ref g1AxisDeccel, value);
                G1AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(280)]
        public double G2AxisVelocity
        {
            get { return g2AxisVelocity; }
            set
            {
                if (g2AxisVelocity == value) return;
                SetRecipe(ref g2AxisVelocity, value);
                G2AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(281)]
        public double G2AxisAccel
        {
            get { return g2AxisAccel; }
            set
            {
                if (g2AxisAccel == value) return;
                SetRecipe(ref g2AxisAccel, value);
                G2AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(282)]
        public double G2AxisDeccel
        {
            get { return g2AxisDeccel; }
            set
            {
                if (g2AxisDeccel == value) return;
                SetRecipe(ref g2AxisDeccel, value);
                G2AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(283)]
        public double G3AxisVelocity
        {
            get { return g3AxisVelocity; }
            set
            {
                if (g3AxisVelocity == value) return;
                SetRecipe(ref g3AxisVelocity, value);
                G3AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(284)]
        public double G3AxisAccel
        {
            get { return g3AxisAccel; }
            set
            {
                if (g3AxisAccel == value) return;
                SetRecipe(ref g3AxisAccel, value);
                G3AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(285)]
        public double G3AxisDeccel
        {
            get { return g3AxisDeccel; }
            set
            {
                if (g3AxisDeccel == value) return;
                SetRecipe(ref g3AxisDeccel, value);
                G3AxisDeccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(286)]
        public double G4AxisVelocity
        {
            get { return g4AxisVelocity; }
            set
            {
                if (g4AxisVelocity == value) return;
                SetRecipe(ref g4AxisVelocity, value);
                G4AxisVelocityChanged?.Invoke(value);
            }
        }

        [ParameterDescription(287)]
        public double G4AxisAccel
        {
            get { return g4AxisAccel; }
            set
            {
                if (g4AxisAccel == value) return;
                SetRecipe(ref g4AxisAccel, value);
                G4AxisAccelChanged?.Invoke(value);
            }
        }

        [ParameterDescription(288)]
        public double G4AxisDeccel
        {
            get { return g4AxisDeccel; }
            set
            {
                if (g4AxisDeccel == value) return;
                SetRecipe(ref g4AxisDeccel, value);
                G4AxisDeccelChanged?.Invoke(value);
            }
        }
    }
}
