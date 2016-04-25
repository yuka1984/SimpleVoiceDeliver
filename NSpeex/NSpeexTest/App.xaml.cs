using System.Windows;

namespace NSpeexTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainViewModel model = new MainViewModel();
            MainView view = new MainView {Model = model};
            model.View = view;
            Current.MainWindow = view;
            Current.MainWindow.Show();
        }

    }
}
