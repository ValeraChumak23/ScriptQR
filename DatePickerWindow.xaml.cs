using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        }

     



        private void ApplyAndContinue_Click(object sender, RoutedEventArgs e)
        {
            // Убедитесь, что обе даты выбраны
            if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                if (StartDatePicker.SelectedDate.Value <= EndDatePicker.SelectedDate.Value)
                {
                    StartDate = StartDatePicker.SelectedDate.Value;
                    EndDate = EndDatePicker.SelectedDate.Value.AddDays(1); // Добавление дня, если это необходимо
                    this.DialogResult = true; // Устанавливаем результат, чтобы окно закрылось
                    Close(); // Явно закрываем окно
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
