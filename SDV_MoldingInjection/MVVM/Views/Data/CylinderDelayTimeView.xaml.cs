using EQX.Core.Recipe;
using EQX.UI.Controls;
using SDV_MoldingInjection.MVVM.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for CylinderDelayTimeView.xaml
    /// </summary>
    public partial class CylinderDelayTimeView : UserControl
    {
        public CylinderDelayTimeView()
        {
            InitializeComponent();
        }

        private void LoadRecipe(IRecipe recipe)
        {
            //Clear Recipe
            CurrentRecipe_StackPanel.Children.Clear();

            CurrentRecipe_StackPanel.Children.Add(new SingleRecipe(null, null) { IsHeader = true });

            int index = 0;

            PropertyInfo[] props = recipe.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                // 1. Get all attributes of property
                List<object> attrs = prop.GetCustomAttributes(false).ToList();

                // 2. Ignore properties of base clas (Head / Name properties)
                if (attrs.Count <= 0) continue;

                // 3. Get DataDescriptionAttribute
                if (attrs.FirstOrDefault(att => (att is SingleRecipeDescriptionAttribute)) == null)
                {
                    continue;
                }
                SingleRecipeDescriptionAttribute dataAttr = (SingleRecipeDescriptionAttribute)attrs.First(att => (att as SingleRecipeDescriptionAttribute) != null);
                if (dataAttr.DescriptionKey != null)
                    dataAttr.Description = Application.Current.Resources[dataAttr.DescriptionKey].ToString();
                if (dataAttr.DetailKey != null)
                    dataAttr.Detail = Application.Current.Resources[dataAttr.DetailKey].ToString();
                // 4. Adding spacer if it's
                if (dataAttr == null)
                {
                    throw new Exception("Attribute need to be add to recipe properties");
                }
                else if (dataAttr.IsSpacer)
                {
                    CurrentRecipe_StackPanel.Children.Add(new SingleRecipe(dataAttr, null));
                    continue;
                }

                // 5. Extract DataMinMaxAtrribute
                SingleRecipeMinMaxAttribute minMaxAttribute = null;
                if (attrs.FirstOrDefault(att => att is SingleRecipeMinMaxAttribute) != null)
                {
                    minMaxAttribute = (SingleRecipeMinMaxAttribute)attrs.FirstOrDefault(att => att is SingleRecipeMinMaxAttribute);
                }

                // 6. Add recipe DataView to the view
                if (prop.PropertyType.Name == nameof(Double)
                    || prop.PropertyType.Name == nameof(Int32) || prop.PropertyType.Name == nameof(UInt32)
                    || prop.PropertyType.Name == nameof(Int64) || prop.PropertyType.Name == nameof(UInt64))
                {
                    dataAttr.Index = ++index;
                    Binding binding = new Binding(prop.Name)
                    {
                        Source = recipe,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };

                    SingleRecipe singleRecipe = new SingleRecipe(dataAttr, minMaxAttribute);
                    singleRecipe.SetBinding(SingleRecipe.ValueProperty, binding);
                    singleRecipe.FontSize = 13;
                    CurrentRecipe_StackPanel.Children.Add(singleRecipe);
                }
                else if (prop.PropertyType.Name == nameof(Boolean))
                {
                    Binding binding = new Binding(prop.Name)
                    {
                        Source = recipe,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    CheckBox checkBox = new CheckBox()
                    {
                        Content = dataAttr.Description,
                        Tag = dataAttr.Detail,
                        Margin = new Thickness(5),
                    };

                    checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);


                    continue;
                }
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is CylinderDelayTimeViewModel dataContext)
            {
                LoadRecipe(dataContext.RecipeList.CylinderDelayTimeRecipe);
            }
        }
    }
}
