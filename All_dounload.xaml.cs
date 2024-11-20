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
    /// Логика взаимодействия для All_dounload.xaml
    /// </summary>
    public partial class All_dounload : Window
    {
        public string CountPerson { get; set; }
        public string Time { get; set; }
        public All_dounload(string count_people, string time)
        {
            
            InitializeComponent();
            CountPerson = count_people;
            Time = time;
            DataContext = this; // обновление данных в окне

        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
