using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace ListWikiApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // global variables
        List<Information> Wiki = new List<Information>();

        #region Utility
        private void DisplayList()
        {
            // clears all items in ListViewOutput
            ListViewOutput.Items.Clear();
            // sorts Wiki by name using IComparable
            Wiki.Sort();

            // adds item elements name and category to the associated ListViewOutput columns
            foreach (var info in Wiki)
            {
                ListViewOutput.Items.Add(new { ColumnName = info.GetName(), ColumnCategory = info.GetCategory() });
            }
        }

        // clears text from and sets focus on TextBoxInput
        private void ClearFocus()
        {
            TextBoxInput.Clear();
            TextBoxInput.Focus();
        }

        // clears Name, Category, Structure and Definition
        private void ClearAll()
        {
            TextBoxName.Clear();
            ComboBoxCategory.SelectedValue = 0;
            RadioButtonLinear.IsChecked = false;
            RadioButtonNonLinear.IsChecked = false;
            TextBoxDefinition.Clear();
        }

        // displays a selected item elements in Name, Category, Structure and Definition
        private void ListViewOutput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TextBoxName.Text = Wiki[ListViewOutput.SelectedIndex].GetName();
                ComboBoxCategory.Text = Wiki[ListViewOutput.SelectedIndex].GetCategory();
                SetStructureRadioButton(ListViewOutput.SelectedIndex);
                TextBoxDefinition.Text = Wiki[ListViewOutput.SelectedIndex].GetDefinition();
            } 
            catch (ArgumentOutOfRangeException) // when an item is edited or deleted
            {
                ClearAll();
                return;
            }
        }

        // calls ClearAll and sets focus on TextBoxName
        private void TextBoxName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClearAll();
            TextBoxName.Focus();
        }
        #endregion

        #region Radio Button Getter & Setter
        // returns string value from selected radio button
        private string GetStructureRadioButton()
        {
            string selectedValue = "";

            if (RadioButtonLinear.IsChecked == true)
            {
                selectedValue = (string)RadioButtonLinear.Content;
            }
            else if (RadioButtonNonLinear.IsChecked == true)
            {
                selectedValue = (string)RadioButtonNonLinear.Content;
            }

            return selectedValue;
        }

        // selects appropriate radio button using structure
        private void SetStructureRadioButton(int index)
        {
            if ((string)RadioButtonLinear.Content == Wiki[index].GetStructure())
            {
                RadioButtonLinear.IsChecked = true;
            }
            else if ((string)RadioButtonNonLinear.Content == Wiki[index].GetStructure())
            {
                RadioButtonNonLinear.IsChecked = true;
            }
        }
        #endregion

        #region Add
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // if Name, Category, Structure and Definition is not empty
            if (!string.IsNullOrWhiteSpace(TextBoxName.Text) && ValidName(TextBoxName.Text)
                && ComboBoxCategory.SelectedIndex > -1 
                && (RadioButtonLinear.IsChecked == true || RadioButtonNonLinear.IsChecked == true)
                && !string.IsNullOrWhiteSpace(TextBoxDefinition.Text))
            {
                // create new Information object, call setters and add object to Wiki
                Information addInfo = new Information();
                addInfo.SetName(TextBoxName.Text);
                addInfo.SetCategory(ComboBoxCategory.Text);
                addInfo.SetStructure(GetStructureRadioButton());
                addInfo.SetDefinition(TextBoxDefinition.Text);
                Wiki.Add(addInfo);

                ClearAll();
                DisplayList();
            } 
            else if (!ValidName(TextBoxName.Text)) // invalid name
            {
                StatusBarInfo.Text = "Name may already exist, or contain numbers or special characters.";
            }
            else // if any input is empty
            {
                StatusBarInfo.Text = "Please complete all fields.";
            }
        }

        // checks if name already exists in Wiki
        private bool ValidName(string input)
        {
            if (Wiki.Exists(duplicate => duplicate.Equals(input))) 
            {
                return false;
            }
            else if (input.Any(char.IsDigit))
            {
                return false;
            }
            else if (input.Any(ch => ! char.IsLetter(ch)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Edit
        // sets focus on TextBoxName
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            TextBoxName.Focus();
            ButtonVisibility(false, true, false); // show ButtonApply and ButtonCancel
        }

        // replaces selected item elements in ListViewOutput with Name, Category, Structure and Definition 
        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            Wiki[ListViewOutput.SelectedIndex].SetName(TextBoxName.Text);
            Wiki[ListViewOutput.SelectedIndex].SetCategory(ComboBoxCategory.Text);
            Wiki[ListViewOutput.SelectedIndex].SetStructure(GetStructureRadioButton());
            Wiki[ListViewOutput.SelectedIndex].SetDefinition(TextBoxDefinition.Text);

            ClearAll();
            ClearFocus();
            DisplayList();
            ButtonVisibility(true, false, true); // hide ButtonApply and ButtonCancel
        }

        // clears selected item in ListViewOutput
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ListViewOutput.SelectedItem = null;
            ClearAll();
            ClearFocus();
            ButtonVisibility(true, false, true); // hide ButtonApply and ButtonCancel
        }

        private void ButtonVisibility(bool a, bool b, bool c)
        {
            // if ButtonEdit is clicked
            if (a == false && b == true)
            {
                ButtonEdit.Visibility = Visibility.Hidden;
                ButtonApply.Visibility = Visibility.Visible;
                ButtonCancel.Visibility = Visibility.Visible;
            }

            // if ButtonApply or ButtonCancel is clicked
            if (a == true && b == false)
            {
                ButtonEdit.Visibility = Visibility.Visible;
                ButtonApply.Visibility = Visibility.Hidden;
                ButtonCancel.Visibility = Visibility.Hidden;
            }

            ButtonOpen.IsEnabled = c;
            ButtonSave.IsEnabled = c;
            ButtonSearch.IsEnabled = c;
            ButtonAdd.IsEnabled = c;
            ButtonDelete.IsEnabled = c;
        }
        #endregion

        #region Delete
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // display prompt
            MessageBoxResult result = MessageBox.Show("Delete this data structure?", "", MessageBoxButton.YesNo);

            // if yes, set selected item elements from ListViewOutput as null
            if (result == MessageBoxResult.Yes)
            {
                Wiki.RemoveAt(ListViewOutput.SelectedIndex);
                ClearAll();
                DisplayList();
            }
        }
        #endregion

        #region Search
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Open
        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenFile(string file)
        {

        }
        #endregion

        #region Save
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveFile(string file)
        {

        }
        #endregion

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // found in ListWikiApp\bin\Debug\net7.0-windows
            ComboBoxCategory.ItemsSource = File.ReadAllLines("PopulateComboBoxCategory.txt");
        }
        #endregion

        #region Closing

        #endregion
    }
}
