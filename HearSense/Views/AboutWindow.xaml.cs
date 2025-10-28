using System.Windows;

namespace HearSense.Views
{
    /// <summary>
    /// Fenêtre "À propos" affichant les informations sur l'application,
    /// le créateur, les avertissements et les technologies utilisées.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
