using ScriptQR.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptQR
{
    public partial class Settings : Window
    {
        private MainWindow MainWindow;
        public string Name_access_key;
        public string Name_division_key;
        public string access_key;
        public string division_key;
        public bool flag_sending;
        public ObservableCollection<KeyValuePair<string, string>> KeyValues { get; set; }

        public Settings(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;
            Name_access_key = MainWindow.Name_Access;
            Name_division_key = MainWindow.Name_Division;
            flag_sending = MainWindow.flag_send;
            KeyValues = new ObservableCollection<KeyValuePair<string, string>>();
            UpdateTextBlocks();
            SendCheckBox.IsChecked = flag_sending;
            StatusTextBlock.Text = flag_sending ? "(Включена)" : "(Отключена)";
        }

        private void UpdateTextBlocks()
        {
            
            KeyValues.Clear();  
            KeyValues.Add(new KeyValuePair<string, string>("Группа доступа", Name_access_key));
            KeyValues.Add(new KeyValuePair<string, string>("Подразделение", Name_division_key));


            KeysDataGrid.ItemsSource = KeyValues;
        }

       public void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem as ListBoxItem;

            if (selectedItem != null)
            {
                // Скрываем все панели перед тем как показать нужную
                KeysDataGrid.Visibility = Visibility.Collapsed;
                SendPanel.Visibility = Visibility.Collapsed;

                // Проверяем, какой пункт выбран
                switch (selectedItem.Content.ToString())
                {
                    case "Подразделение/Группа доступа":
                        KeysDataGrid.Visibility = Visibility.Visible;
                        break;
                    case "Отправка QR по почте":
                        SendPanel.Visibility = Visibility.Visible;
                        break;
                }
            }
        }


    

       
        public void EditKey_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = FindParent<DataGridRow>(button);

            if (row != null)
            {
                var selectedItem = row.Item as KeyValuePair<string, string>?;

                if (selectedItem.HasValue)
                {
                    var keyValue = selectedItem.Value;

                    if (keyValue.Key == "Группа доступа")
                    {
                         EditAccessKey_Click();
                    }
                    else if (keyValue.Key == "Подразделение")
                    {
                        EditDivisionKey_Click();
                    }
                }
            }
        }

        // Метод для поиска родительского элемента (строки)
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }


        public async void EditAccessKey_Click()
        {
            try
            {
                var Session_ID = await MainWindow.OpenSession();
                if (Session_ID != Guid.Empty)
                {
                    var client = new IntegrationServiceSoapClient();
                    var result = await client.GetAccessGroupsAsync(Session_ID);
                    var accsessGroup = result.Body.GetAccessGroupsResult;
                    var windows_choice = new Window_for_choice(accsessGroup);

                    if (windows_choice.ShowDialog() == true) // Ожидание выбора
                    {
                        Name_access_key = windows_choice.SelectedUnit.Name;
                        access_key = windows_choice.SelectedUnit.Key;
                        MainWindow.Name_Access = Name_access_key.ToString();
                        MainWindow.Key_Access = access_key.ToString();
                        UpdateTextBlocks();  
                        await MainWindow.LogMessage($"Данные о группе доступа успешно изменены.\nИмя группы доступа:{Name_access_key}", "system");
                    }
                    else
                    {
                        await MainWindow.LogMessage($"Выбор не был сделан.", "system");
                        MessageBox.Show("Выбор не был сделан.");
                    }
                    MainWindow.ModifyXmlFile(@"C:\ScriptQR\default_data.xml");
                    await MainWindow.CloseSession();
                }
            }
            catch (Exception ex)
            {
                await MainWindow.LogMessage($"Произошла ошибка при получении списка групп доступа.\n{ex.Message}", "system");
            }
        }

        public async void EditDivisionKey_Click()
        {
            try
            {
                var Session_ID = await MainWindow.OpenSession();
                var client = new IntegrationServiceSoapClient();
                var result = await client.GetOrgUnitsHierarhyAsync(Session_ID);
                var orgUnits = result.Body.GetOrgUnitsHierarhyResult;

                var windows_choice = new Window_for_choice(orgUnits);

                if (windows_choice.ShowDialog() == true) 
                {
                    Name_division_key = windows_choice.SelectedUnit.Name;
                    division_key = windows_choice.SelectedUnit.Key;
                    MainWindow.Name_Division = Name_division_key.ToString();
                    MainWindow.Key_Division = division_key.ToString();
                    UpdateTextBlocks();  
                    await MainWindow.LogMessage($"Данные о подразделении успешно изменены.\nИмя подразделения:{Name_division_key}", "system");
                }
                else
                {
                    await MainWindow.LogMessage($"Выбор не был сделан.", "system");
                    MessageBox.Show("Выбор не был сделан.");
                }

                MainWindow.ModifyXmlFile(@"C:\ScriptQR\default_data.xml");
                await MainWindow.CloseSession();
            }
            catch (Exception ex)
            {
                await MainWindow.LogMessage($"Произошла ошибка при получении списка подразделений.\n{ex.Message}", "system");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            flag_sending = true;
            MainWindow.flag_send = flag_sending;
            StatusTextBlock.Text = "(Включена)";
            MainWindow.ModifyXmlFile(@"C:\ScriptQR\default_data.xml");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            flag_sending = false;
            MainWindow.flag_send = flag_sending;
            StatusTextBlock.Text = "(Отключена)";
            MainWindow.ModifyXmlFile(@"C:\ScriptQR\default_data.xml");
        }
    }
}
