using System;
using System.Windows;

namespace ScriptQR
{
    public partial class DatePickerWindow : Window
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool ApplyForAll { get; private set; }

        public DatePickerWindow()
        {
            InitializeComponent();
            this.Left = App.WindowLeft;  // Используем глобальные координаты
            this.Top = App.WindowTop;
        }

        private void ApplyAndContinue_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                if (StartDatePicker.SelectedDate.Value < EndDatePicker.SelectedDate.Value)
                {
                    StartDate = StartDatePicker.SelectedDate.Value;
                    EndDate = EndDatePicker.SelectedDate.Value;
                    ApplyForAll = false; // Применить только для текущей строки
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Дата начала должна быть меньше даты конца!");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите обе даты.");
            }
        }
    }
}
