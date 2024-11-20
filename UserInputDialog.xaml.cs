using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ScriptQR
{
    /// <summary>
    /// Логика взаимодействия для UserInputDialog.xaml
    /// </summary>
    public partial class UserInputDialog : Window
    {
        public string userName;
        public string password;
        public string organization;

        public UserInputDialog()
        {
            InitializeComponent();
            

            this.Left = App.WindowLeft;  
            this.Top = App.WindowTop;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Собираем список незаполненных полей
            List<string> emptyFields = new List<string>();

            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                emptyFields.Add("Имя пользователя");
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                emptyFields.Add("Пароль");
            }

            if (string.IsNullOrWhiteSpace(OrganizationTextBox.Text))
            {
                emptyFields.Add("Подразделение");
            }

            // Проверяем, есть ли пустые поля
            if (emptyFields.Count > 0)
            {
                // Формируем сообщение с перечнем незаполненных полей
                string message = "Процесс не может быть продолжен, пока не будет заполнено поле(я): " + string.Join(", ", emptyFields);

                // Выводим сообщение об ошибке
                MessageBox.Show(message, "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Если все поля заполнены, продолжаем обработку данных
                userName = UserNameTextBox.Text;
                password = PasswordBox.Password;
                organization = OrganizationTextBox.Text;

                // Выводим введенные данные (это можно убрать, если не нужно)
               // MessageBox.Show($"Имя пользователя: {userName}\nПароль: {password}\nПодразделение: {organization}", "Введенные данные");

                // Устанавливаем успешный результат диалога и закрываем окно
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
