using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace TestApplication.Windows
{
    /// <summary>
    /// Логика взаимодействия для DatabaseConfigurationWindow.xaml
    /// </summary>
    public partial class DatabaseConfigurationWindow : Window
    {
        public DatabaseConfigurationWindow()
        {
            InitializeComponent();
            var host = ConfigurationManager.AppSettings["postgresql_host"];
            var port = ConfigurationManager.AppSettings["postgresql_port"];
            var db = ConfigurationManager.AppSettings["postgresql_database"];
            var user = ConfigurationManager.AppSettings["postgresql_username"];
            var password = ConfigurationManager.AppSettings["postgresql_password"];
            PostgresHost.Text = host;
            PostgresPort.Text = port;
            PostgresDatabase.Text = db;
            PostgresUser.Text = user;
            PostgresPassword.Text = password;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["postgresql_host"].Value = PostgresHost.Text;
            config.AppSettings.Settings["postgresql_port"].Value = PostgresPort.Text;
            config.AppSettings.Settings["postgresql_database"].Value = PostgresDatabase.Text;
            config.AppSettings.Settings["postgresql_username"].Value = PostgresUser.Text;
            config.AppSettings.Settings["postgresql_password"].Value = PostgresPassword.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PostgresPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            int port = 0;
            if(int.TryParse(PostgresPort.Text,out port))
            {
                PostgresPort.Text = (port & 65535).ToString();
            }
            else
            {
                PostgresPort.Text = new string(PostgresPort.Text.Where(n => char.IsDigit(n)).ToArray());
            }
        }
    }
}
