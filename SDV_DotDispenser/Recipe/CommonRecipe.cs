using EQX.Core.Recipe;
using EQX.Core.Units;
using EQX.UI.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Recipe
{
    public class CommonRecipe : RecipeBase
    {
        private double cylinderMoveTimeout;
        private double motionOriginTimeout;
        private double motionMoveTimeout;
        private double vacDelay;
        private int logSaveDay = 30;
        public event Action<ILanguageDefinition> SelectedLanguageEvent;
        public event Action SelectedLanguageLoadAllRecipe;
        [SingleRecipeDescription(Description = "Cylinder Move Timeout", Unit = Unit.Second)]
        public double CylinderMoveTimeout
        {
            get { return cylinderMoveTimeout; }
            set
            {
                if (cylinderMoveTimeout == value) return;

                OnRecipeChanged(cylinderMoveTimeout, value);
                cylinderMoveTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Motion Origin Timeout", Unit = Unit.Second)]
        public double MotionOriginTimeout
        {
            get { return motionOriginTimeout; }
            set
            {
                if (motionOriginTimeout == value) return;

                OnRecipeChanged(motionOriginTimeout, value);
                motionOriginTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Motion move timeout", Unit = Unit.Second)]
        public double MotionMoveTimeout
        {
            get { return motionMoveTimeout; }
            set
            {
                if (motionMoveTimeout == value) return;

                OnRecipeChanged(motionMoveTimeout, value);
                motionMoveTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Vacuum Delay", Unit = Unit.Second)]
        public double VacDelay
        {
            get { return vacDelay; }
            set
            {
                if (vacDelay == value) return;

                OnRecipeChanged(vacDelay, value);
                vacDelay = value;
            }
        }

        private double materialInputTimeout = 1800.0;

        [SingleRecipeDescription(Description = "Material Input Timeout",
            Detail = "Time Machine Not Have Material To Stop", Unit = Unit.Second)]
        public double MaterialInputTimeout
        {
            get { return materialInputTimeout; }
            set
            {
                OnRecipeChanged(materialInputTimeout, value);
                materialInputTimeout = value;
            }
        }


        [SingleRecipeDescription(Description = "Log Save Day")]
        [SingleRecipeMinMax(Max = 100, Min = 5)]
        public int LogSaveDay
        {
            get { return logSaveDay; }
            set 
            {
                OnRecipeChanged(logSaveDay, value);
                logSaveDay = value; 
            }
        }

        [SingleRecipeDescription(
            Description = "Skip Left Port",
            Detail = "Check to Skip Left Port")]
        public bool DisableLeftPort
        {
            get { return disableLeftPort; }
            set
            {
                if (disableLeftPort == value) return;

                OnRecipeChanged(disableLeftPort, value);
                disableLeftPort = value;
            }
        }

        [SingleRecipeDescription(
            Description = "Skip Right Port",
            Detail = "Check to Skip Right Port")]
        public bool DisableRightPort
        {
            get { return disableRightPort; }
            set
            {
                if (disableRightPort == value) return;

                OnRecipeChanged(DisableRightPort, value);
                disableRightPort = value;
            }
        }

        [SingleRecipeDescription(
            Description = "Skip Vinyl Clean",
            Detail = "Check to skip Vinyl Clean")]
        public bool SkipVinylClean
        {
            get { return skipVinylClean; }
            set
            {
                if (skipVinylClean == value) return;

                OnRecipeChanged(skipVinylClean, value);
                skipVinylClean = value;
            }
        }
        
        public ILanguageDefinition SelectedLanguage
        {
            get => selectLanguage;
            set
            {
                if (selectLanguage == value) return;
                OnRecipeChanged(selectLanguage, value);
                selectLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                SelectedLanguageEvent?.Invoke(SelectedLanguage);
                SelectedLanguageLoadAllRecipe?.Invoke();
            }
        }
        #region Privates
        private bool disableLeftPort;
        private bool disableRightPort;
        private bool skipVinylClean;
        private ILanguageDefinition selectLanguage;
        #endregion
    }
}
