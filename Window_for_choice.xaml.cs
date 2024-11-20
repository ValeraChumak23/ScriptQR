using ScriptQR.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ScriptQR
{
    public partial class Window_for_choice : Window
    {
        public class Unit
        {
            public int Number { get; set; }
            public string Name { get; set; }
            public string Key { get; set; }
        }

        public Unit SelectedUnit { get; private set; }

        private List<Unit> Unit_list; // Поле класса для хранения списка

        // Конструктор для массива OrgUnit
        public Window_for_choice(OrgUnit[] OrgUnit_arr)
        {
            InitializeComponent();
            this.KeyDown += Window_for_choice_KeyDown;
            PopulateList(OrgUnit_arr);
            this.Left = App.WindowLeft;
            this.Top = App.WindowTop;
        }

        // Конструктор для массива AccessGroup
        public Window_for_choice(AccessGroup[] AccessGroup_arr)
        {
            InitializeComponent();
            this.KeyDown += Window_for_choice_KeyDown;
            PopulateList(AccessGroup_arr);
            this.Left = App.WindowLeft;
            this.Top = App.WindowTop;
        }

        // Метод для заполнения списка подразделений (OrgUnit)
        private void PopulateList(OrgUnit[] OrgUnit_arr)
        {
            int num = 1;
            Unit_list = new List<Unit>(); // Используем поле класса
            foreach (var unit in OrgUnit_arr)
            {
                var OrgUnit = new Unit
                {
                    Number = num,
                    Name = unit.NAME,
                    Key = unit.ID.ToString()
                };
                Unit_list.Add(OrgUnit);
                num++;
            }
            UnitListView.ItemsSource = Unit_list; // Привязываем данные к ListView
            FoundItemsLabel.Content = $"Найдено: {Unit_list.Count}";
        }

        // Метод для заполнения списка групп доступа (AccessGroup)
        private void PopulateList(AccessGroup[] AccessGroup_arr)
        {
            int num = 1;
            Unit_list = new List<Unit>(); // Используем поле класса
            foreach (var group in AccessGroup_arr)
            {
                var accessGroup = new Unit
                {
                    Number = num,
                    Name = group.NAME,
                    Key = group.ID.ToString()
                };
                Unit_list.Add(accessGroup);
                num++;
            }
            UnitListView.ItemsSource = Unit_list; // Привязываем данные к ListView
            FoundItemsLabel.Content = $"Найдено: {Unit_list.Count}";
        }

        // Обработчик нажатия кнопки выбора
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedUnit = (Unit)UnitListView.SelectedItem;

            if (SelectedUnit != null)
            {
                this.DialogResult = true;  // Устанавливаем успешный результат
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите строку для продолжения");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Указываем, что выбор не был сделан
            this.Close();
        }

        private void SearchTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            PerformSearch(); // Выполнить поиск
        }

        private void PerformSearch()
        {
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                var searchText = SearchTextBox.Text.ToLower();
                var filteredItems = Unit_list.Where(unit => unit.Name.ToLower().Contains(searchText)).ToList();
                UnitListView.ItemsSource = filteredItems;
                FoundItemsLabel.Content = $"Найдено: {filteredItems.Count}";
            }
            else
            {
                UnitListView.ItemsSource = Unit_list;
                FoundItemsLabel.Content = $"Найдено: {Unit_list.Count}";
            }
        }

        private void Window_for_choice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleSearchBoxVisibility();
            }
        }
        private void ToggleSearchBoxVisibility()
        {
            if (SearchPanel.Visibility == Visibility.Collapsed)
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchTextBox.Focus(); 
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
