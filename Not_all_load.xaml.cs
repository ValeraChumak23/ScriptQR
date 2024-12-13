using ScriptQR.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static ScriptQR.MainWindow;
using static ScriptQR.Window_for_choice;

namespace ScriptQR
{
    /// <summary>
    /// Логика взаимодействия для Not_all_load.xaml
    /// </summary>
    public partial class Not_all_load : Window
    {
        public string CountPerson { get; set; }
        public string Not_load_CountPerson { get; set; }
        public string Time { get; set; }
        public string FilePath { get; set; }

        public struct Info_person_list
        {
            public string FIO { get; set; }
            public string Number_phone { get; set; }
        }

        public List<Info_person_list> info_about_people;

        public struct Not_load_person_list
        {
            public int Number { get; set; }
            public string FIO { get; set; }
        }

        public Not_all_load(List<PersonData> list_person,string count_person,string time, string filepath)
        {
            InitializeComponent();
            CountPerson = count_person;
            Not_load_CountPerson = (list_person.Count).ToString();
            Time = time;
            FilePath = filepath;

            int num = 1;
            var person_list = new List<Not_load_person_list>();
            info_about_people = new List<Info_person_list>();
            foreach (var person in list_person)
            {
                string FIO = person.LastName + " " + person.FirstName + " " + person.MiddleName;
                var person_not_load = new Not_load_person_list
                {
                    Number = num,
                    FIO = FIO
                };
                var info_about_person = new Info_person_list
                {
                    FIO = FIO,
                    Number_phone = person.Phone_number
                };
                num++;
                person_list.Add(person_not_load);
                info_about_people.Add(info_about_person);
            }
            UnitListView.ItemsSource = person_list;
            DataContext = this;

           

        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var send_window = new Send_window(info_about_people);
            send_window.Show();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string path = e.Uri.LocalPath;

                if (File.Exists(path))
                {
                    // Открыть файл
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (Directory.Exists(path))
                {
                    // Открыть папку
                    Process.Start(new ProcessStartInfo(path) { FileName = path, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Путь недействителен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии пути: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true; // Указываем, что событие обработано
        }


    }
}
