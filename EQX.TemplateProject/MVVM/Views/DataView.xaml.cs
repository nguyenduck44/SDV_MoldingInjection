using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.UI.Controls;
using OpenCvSharp.Flann;
using EQX.TemplateProject.MVVM.ViewModels;
using EQX.TemplateProject.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EQX.TemplateProject.MVVM.Views
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        public DataView()
        {
            InitializeComponent();
        }

        private void ReloadButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_ReloadAllData"]) == true)
            {
                if (this.DataContext is DataViewModel dataContext)
                {
                    dataContext.RecipeSelector.Load();
                    LoadRecipe(dataContext.CurrentRecipe);
                }
            }
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_ChangeModel"]) == true)
            {
                if (this.DataContext is DataViewModel dataContext)
                {
                    dataContext.RecipeSelector.SetCurrentModel(dataContext.SelectedModel);
                    LoadRecipe(dataContext.CurrentRecipe);
                }
            }
        }

        private void RecipeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            if (e.AddedItems[0] is not IRecipe recipe) return;

            LoadRecipe(recipe);
        }

        private void LoadRecipe(IRecipe recipe)
        {
            //Clear Recipe
            CurrentRecipe_StackPanel.Children.Clear();
            OptionsRecipe_StackPanel.Children.Clear();

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

                    OptionsRecipe_StackPanel.Children.Add(checkBox);

                    

                    continue;
                }
            }
        }


        private void DataView_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.DataContext is not DataViewModel viewModel) return;

            viewModel.LoadRecipeEvent += () =>
            {
                if (viewModel.CurrentRecipe != null)
                {
                    LoadRecipe(viewModel.CurrentRecipe);
                }
            };
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {

            if (sender is not ListBox listBox) return;
            if (this.DataContext is not DataViewModel viewModel) return;

            // Set selected recipe based on binding
            if (viewModel.SelectedRecipe != null)
            {
                listBox.SelectedItem = viewModel.SelectedRecipe;
            }
            else if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            // Focus on selected item
            if (listBox.SelectedItem != null)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
                if (item != null)
                {
                    item.Focus();
                }
            }
        }

        private void ModelsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            if (this.DataContext is not DataViewModel viewModel) return;

            // Load from current recipe and set back to SelectedModel
            if (viewModel.RecipeSelector?.RecipeSetting?.CurrentRecipe != null)
            {
                string currentRecipeName = viewModel.RecipeSelector.RecipeSetting.CurrentRecipe;
                viewModel.SelectedModel = currentRecipeName;
                listBox.SelectedItem = currentRecipeName;
            }
            else if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            // Focus on selected item
            if (listBox.SelectedItem != null)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
                if (item != null)
                {
                    item.Focus();
                }
            }
        }
    }
}
