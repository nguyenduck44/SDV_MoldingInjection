using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using EQX.TemplateProject.Recipe;
using EQX.TemplateProject.Services.Interlock;

namespace EQX.TemplateProject.MVVM.ViewModels.Teaching
{
    public class UnitTeachingViewModel : ViewModelBase, INameable
    {
        public UnitTeachingViewModel(string name, RecipeSelector recipeSelector)
        {
            Name = name;
            RecipeSelector = recipeSelector;
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (Inputs == null) return;

            foreach (var input in Inputs)
            {
                input.RaiseValueUpdated();
            }
        }

        public ObservableCollection<ICylinder> Cylinders { get; set; }
        public ObservableCollection<IMotion> Motions { get; set; }
        public ObservableCollection<IDInput> Inputs { get; set; }
        public ObservableCollection<IDOutput> Outputs { get; set; }
        public IRecipe Recipe { get; set; }
        public string Name { get; init; }
        public RecipeSelector RecipeSelector { get; }
        public ImageSource Image { get; set; }

        public ICommand MovePositionTeachingCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (o is SinglePositionTeaching spt == false) return;
                    string MoveTo = (string)Application.Current.Resources["str_MoveTo"];

                    string motionName = spt.SinglePosition.Motion;
                    string moveToDescription = spt.SingleRecipeDescription.Description;

                    if (MessageBoxEx.ShowDialog($"{MoveTo} {moveToDescription} ?") == true)
                    {
                        InterlockService interlockService = App.AppHost!.Services.GetRequiredService<InterlockService>();

                        // Check Interlocks beforee move
                        if (interlockService.InterlockCheck(Name, moveToDescription) == false)
                        {
                            return;
                        }
                        Motions.FirstOrDefault(m => m.Name.Contains(motionName))!.MoveAbs(spt.Value);
                    }
                });
            }
        }

        public ICommand GetCurrentPositionCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (o is SinglePositionTeaching spt == false) return;

                    string motionName = spt.SinglePosition.Motion;

                    spt.Value = Motions.FirstOrDefault(m => m.Name.Contains(motionName))!.Status.ActualPosition;
                });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (MessageBoxEx.ShowDialog($"{(string)Application.Current.Resources["str_Save"]}?") == true)
                    {
                        RecipeSelector.Save();
                    }
                });
            }
        }

        public ICommand CylinderForwardCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (o is ICylinder cylinder == false) return;
                    if (cylinder.CylinderType == ECylinderType.ForwardBackwardReverse ||
                        cylinder.CylinderType == ECylinderType.UpDownReverse ||
                        cylinder.CylinderType == ECylinderType.RightLeftReverse ||
                        cylinder.CylinderType == ECylinderType.GripUngripReverse ||
                        cylinder.CylinderType == ECylinderType.AlignUnalignReverse ||
                        cylinder.CylinderType == ECylinderType.LockUnlockReverse ||
                        cylinder.CylinderType == ECylinderType.FlipUnflipReverse ||
                        cylinder.CylinderType == ECylinderType.ClampUnclampReverse
                        )
                    {
                        cylinder.Backward();
                        return;
                    }
                    cylinder.Forward();
                });
            }
        }

        public ICommand CylinderBackwardCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (o is ICylinder cylinder == false) return;

                    if (cylinder.CylinderType == ECylinderType.ForwardBackwardReverse ||
                        cylinder.CylinderType == ECylinderType.UpDownReverse ||
                        cylinder.CylinderType == ECylinderType.RightLeftReverse ||
                        cylinder.CylinderType == ECylinderType.GripUngripReverse ||
                        cylinder.CylinderType == ECylinderType.AlignUnalignReverse ||
                        cylinder.CylinderType == ECylinderType.LockUnlockReverse ||
                        cylinder.CylinderType == ECylinderType.FlipUnflipReverse ||
                        cylinder.CylinderType == ECylinderType.ClampUnclampReverse)
                    {
                        cylinder.Forward();
                        return;
                    }
                    cylinder.Backward();
                });
            }
        }
    }
}
