namespace SoarCraft.QYun.ArknightsAssetStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;

    // TODO WTS: Change the grid as appropriate to your app, adjust the column definitions on DataGridPage.xaml.
    // For more details see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid
    public sealed partial class DataGridPage : Page {
        public DataGridViewModel ViewModel { get; }

        public DataGridPage() {
            this.ViewModel = Ioc.Default.GetService<DataGridViewModel>();
            this.InitializeComponent();
        }
    }
}
