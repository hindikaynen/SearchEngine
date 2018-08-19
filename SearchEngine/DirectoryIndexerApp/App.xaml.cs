using System.Windows;

namespace DirectoryIndexerApp
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var view = new MainWindow();
            var viewModel = new MainWindowViewModel(new DialogService(), new DispatcherImpl(Dispatcher));
            view.DataContext = viewModel;
            view.ShowDialog();
        }
    }
}
