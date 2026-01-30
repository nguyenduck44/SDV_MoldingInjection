using CommunityToolkit.Mvvm.ComponentModel;

namespace EQX.TemplateProject.MVVM.Models
{
    public class ManualVMWithSelection : ObservableObject
    {
        public string UnitName
        {
            get { return _UnitName; }
            set
            {
                _UnitName = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                OnPropertyChanged();
            }
        }

        public ManualVMWithSelection(string name, bool selected)
        {
            UnitName = name;
            IsSelected = selected;
        }

        #region Privates
        private string _UnitName = "";
        private bool _IsSelected;
        #endregion
    }
}
