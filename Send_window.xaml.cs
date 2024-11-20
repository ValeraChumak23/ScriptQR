using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
using static ScriptQR.Not_all_load;

namespace ScriptQR
{
    /// <summary>
    /// Логика взаимодействия для Send_window.xaml
    /// </summary>
    public partial class Send_window : Window
    {
        public string email_recipient;

        public List<Info_person_list> info_about_people;

        public MainWindow MainWindow;


        public Send_window(List<Info_person_list> info_list)
        {
            InitializeComponent();
            info_about_people = info_list;
        }

        public async void Send_Click(object sender, RoutedEventArgs e)
        {

            email_recipient = Recipient.Text;

            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string fromEmail = "estr65st@gmail.com";
            string password = "yrnw tpwm unqt fyot";
            string toEmail = email_recipient;

            // Создаем письмо
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail);
            mail.To.Add(new MailAddress(toEmail));
            mail.Subject = "Список людей у которых есть проблемы с данными";



            StringBuilder htmlBody = new StringBuilder();
            htmlBody.Append("<p>Список людей не загруженых в СКУД по причине не коректно введеной почты или ее отсутствия:</p>");
            htmlBody.Append("<ul>");

            foreach (var info_about_person in info_about_people)
            {
                htmlBody.Append($"<li>{info_about_person.FIO},{info_about_person.Number_phone}</li>");
            }

            htmlBody.Append("</ul>");

            // Устанавливаем тело письма как HTML
            mail.Body = htmlBody.ToString();
            mail.IsBodyHtml = true; // Указываем, что тело письма содержит HTML

            // Настройки SMTP-клиента
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(fromEmail, password);
            smtpClient.EnableSsl = true;

            try
            {
                // Отправляем письмо асинхронно
                await smtpClient.SendMailAsync(mail);

                //await MainWindow.LogMessage($"Письмо со списком не загруженных отправлено по адресу {email_recipient}");

            }
            catch (SmtpException smtpEx)
            {
                if (smtpEx.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    MessageBox.Show("Почтовый ящик не существует.");
                }
                else
                {
                    MessageBox.Show($"Ошибка при отправке письма: {smtpEx.Message}");
                }
                //MessageBox.Show($"Ошибка при отправке письма: {ex.Message}");
                //await MainWindow.LogMessage($"Ошибка при отправке письма по адресу {email_recipient}.\n{ex.Message}");

            }
            finally
            {
                mail.Dispose();
                smtpClient.Dispose();
            }

            this.Close();

        }
    }
    
}
