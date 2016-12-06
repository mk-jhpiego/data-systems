using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace desktop_builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        moduleDefinition _currentModule = null;
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void closeButton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void setValue(TextBox control, string textOption)
        {
            textOption = string.IsNullOrWhiteSpace(textOption) ? "" : textOption;
            control.Text = textOption;
        }

        void setValue(string textOption, params RadioButton[] controls)
        {
            foreach (var control in controls)
            {
                control.IsChecked = false;
            }

            if (string.IsNullOrWhiteSpace(textOption)) return;

            foreach (var control in controls)
            {
                if (control.Content.ToString() == textOption)
                {
                    control.IsChecked = true;
                    break;
                }
            }
        }

        string getValue(TextBox control)
        {
            return control.Text ?? control.Text;
        }

        string getCheckedItem(params RadioButton[] controls)
        {
            var selectedControl = controls.Where(control => control.IsChecked != null && control.IsChecked == true)
                .FirstOrDefault();
            return selectedControl == null ? "" : selectedControl.Content.ToString();
        }

        void updateModuleDefinition(moduleDefinition module)
        {
            module.name = getValue(textModuleName);
            module.description = getValue(textModuleDescription);
            module.version = getValue(textModuleVersion);
            var moduleTypeOption = getCheckedItem(
                rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            module.moduleType = moduleTypeOption;
            module.userId = getValue(textEmailAddress);
        }

        void updateView(moduleDefinition module)
        {
            _currentModule = module;
            setValue(textModuleName, module.name);
            setValue(textModuleDescription, module.description);
            setValue(textModuleVersion, module.version);
            setValue(module.moduleType, rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            setValue(textEmailAddress, module.userId);
            setValue(textModuleName, module.name);
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {
            //we update the module definition
            _currentModule = _currentModule ?? new moduleDefinition() { id = Guid.NewGuid().ToString("N") };
            updateModuleDefinition(_currentModule);

            //and save
            var name = _currentModule.name.Replace(' ', '-').ToLowerInvariant() + ".json";
            var dialog = new SaveFileDialog() { FileName = name, OverwritePrompt = true, Filter = "*.json|json", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult == true)
            {
                File.WriteAllText(dialog.FileName,
                    Newtonsoft.Json.JsonConvert.SerializeObject(_currentModule));
            }
        }

        //addField_click
        private void addField_click(object sender, RoutedEventArgs e) {
            //weread the field values and clear the form

        }
        //cancelAddField_click
        private void cancelAddField_click(object sender, RoutedEventArgs e) {
            //discard changes and clear the fields

        }

        private void saveSubModuleChangesButton_click(object sender, RoutedEventArgs e)
        {
            //weread the field values and clear the form

        }
        private void cancelSubModuleChanges_click(object sender, RoutedEventArgs e)
        {
            //discard changes and clear the fields

        }

        private void openExisting_click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { CheckFileExists = true,
                Multiselect =false, Filter="JSON|*.json",
                InitialDirectory =Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments) };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult == true)
            {
                //we open the file
                var savedModule =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<moduleDefinition>
                    (File.ReadAllText(dialog.FileName));
                updateView(savedModule);
            }
        }

        private void cancelButton_click(object sender, RoutedEventArgs e)
        {
            //warn that they wil lose data before attempting to close
            var msg = MessageBox.Show("Do you want to close? Unsaved changes will be lost","Confirm action", MessageBoxButton.OKCancel);
            if (msg == MessageBoxResult.OK)
            {
                var module = new moduleDefinition() { id = Guid.NewGuid().ToString("N") };
                updateView(module);
            }            
        }
    }
}
