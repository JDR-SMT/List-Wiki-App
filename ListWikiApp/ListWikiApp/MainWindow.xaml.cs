using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            // if Name, Category, Structure and Definition is not empty and ValidName is true
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

        // checks if name already exists in Wiki, or contains a number or special character
        private bool ValidName(string input)
        {
            if (Wiki.Exists(duplicate => duplicate.GetName().Equals(input)))
            {
                return false;
            }
            else if (input.Any(char.IsDigit))
            {
                return false;
            }
            else if (input.Any(ch => !char.IsLetter(ch) && !char.IsWhiteSpace(ch)))
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
            try
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
            catch (ArgumentOutOfRangeException) // when no item is selected
            {
                StatusBarInfo.Text = "Please select an item to edit.";
                ClearAll();
                ButtonVisibility(true, false, true); // hide ButtonApply and ButtonCancel
                return;
            }
        }

        // clears selected item in ListViewOutput
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ListViewOutput.SelectedItem = null;
            ClearAll();
            ClearFocus();
            ButtonVisibility(true, false, true); // hide ButtonApply and ButtonCancel
        }

        private void ButtonVisibility(bool buttonEdit, bool buttonApplyCancel, bool enable)
        {
            // if ButtonEdit is clicked
            if (buttonEdit == false && buttonApplyCancel == true)
            {
                ButtonEdit.Visibility = Visibility.Hidden;
                ButtonApply.Visibility = Visibility.Visible;
                ButtonCancel.Visibility = Visibility.Visible;
            }

            // if ButtonApply or ButtonCancel is clicked
            if (buttonEdit == true && buttonApplyCancel == false)
            {
                ButtonEdit.Visibility = Visibility.Visible;
                ButtonApply.Visibility = Visibility.Hidden;
                ButtonCancel.Visibility = Visibility.Hidden;
            }

            ButtonOpen.IsEnabled = enable;
            ButtonSave.IsEnabled = enable;
            ButtonSearch.IsEnabled = enable;
            ButtonAdd.IsEnabled = enable;
            ButtonDelete.IsEnabled = enable;
        }
        #endregion

        #region Delete
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewOutput.SelectedIndex >= 0)
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
            else // when no item is selected
            {
                StatusBarInfo.Text = "Please select an item to delete.";
                ClearAll();
                return;
            }
        }
        #endregion

        #region Search
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            // TextBoxInput is not empty
            if (!string.IsNullOrWhiteSpace(TextBoxInput.Text))
            {
                // sorts Wiki to ensure BinarySearch is functional
                Wiki.Sort();

                // create new Information object, call SetName with TextBoxInput.Text
                Information findInfo = new Information();
                findInfo.SetName(TextBoxInput.Text);

                // returns index of TextBoxInput in Wiki
                int index = Wiki.BinarySearch(findInfo);

                if (index >= 0) // found
                {
                    StatusBarInfo.Text = TextBoxInput.Text + " found.";
                    ListViewOutput.SelectedIndex = index;
                    ClearFocus();
                }
                else // not found
                {
                    StatusBarInfo.Text = TextBoxInput.Text + " not found.";
                    ListViewOutput.SelectedIndex = -1;
                    ClearFocus();
                }
            }
            else // TextBoxInput is empty
            {
                StatusBarInfo.Text = "Please enter a word to search.";
                ClearFocus();
            }
        }
        #endregion

        #region Open
        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            // displays prompt to open a data file
            OpenFileDialog ofd = new OpenFileDialog { Filter = "data files (*.dat)|*.dat" };

            // if ok
            if (ofd.ShowDialog() == true)
            {
                // calls OpenFile with selected file
                OpenFile(ofd.FileName);

                DisplayList();
            }
            else
            { // if cancel or exit
                return;
            }
        }

        private void OpenFile(string file)
        {
            try
            {
                // clears all elements in Wiki
                Wiki.Clear();

                // reads file
                using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open)))
                {
                    // while position does not equal length of BaseStream
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        // create new Information object, call setters and add object to Wiki
                        Information addInfo = new Information();
                        addInfo.SetName(br.ReadString());
                        addInfo.SetCategory(br.ReadString());
                        addInfo.SetStructure(br.ReadString());
                        addInfo.SetDefinition(br.ReadString());
                        Wiki.Add(addInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception thrown: " + ex, "Critical Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Save
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (Wiki.Count > 0)
            {
                // displays prompt to save Wiki to a data file
                SaveFileDialog sfd = new SaveFileDialog
                {
                    DefaultExt = "dat",
                    Filter = "data files (*.dat)|*.dat"
                };

                // if ok
                if (sfd.ShowDialog() == true)
                {
                    DisplayList();

                    // calls SaveFile with selected file
                    SaveFile(sfd.FileName);
                }
                else
                { // if cancel or exit
                    return;
                }
            }
            else // when Wiki is empty
            {
                StatusBarInfo.Text = "Please enter data to save.";
                return;
            }
        }

        private void SaveFile(string file)
        {
            try
            {
                // creates file
                using (BinaryWriter bw = new BinaryWriter(new FileStream(file, FileMode.Create)))
                {
                    // adds all item elements into a data file
                    foreach (var info in Wiki)
                    {
                        bw.Write(info.GetName());
                        bw.Write(info.GetCategory());
                        bw.Write(info.GetStructure());
                        bw.Write(info.GetDefinition());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception thrown: " + ex, "Critical Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // output in ListWikiApp\bin\Debug\net7.0-windows
            SaveFile("autosave.dat");
        }
        #endregion
    }
}