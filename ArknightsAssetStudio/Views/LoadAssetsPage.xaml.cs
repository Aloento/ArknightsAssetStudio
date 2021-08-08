namespace SoarCraft.QYun.ArknightsAssetStudio.Views {
    using System;
    using Windows.Storage.Pickers;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using ViewModels;
    using WinRT.Interop;
    using Microsoft.UI.Xaml.Input;
    using System.Threading.Tasks;

    public sealed partial class LoadAssetsPage : Page {
        public LoadAssetsModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        public LoadAssetsPage() {
            this.ViewModel = Ioc.Default.GetService<LoadAssetsModel>();
            this.InitializeComponent();

#if DEBUG
            logger.Debug($"Loading {nameof(LoadAssetsPage)}");
#endif
        }

        private void LoadPanel_OnLoaded(object sender, RoutedEventArgs e) {
            #region CommandBar

            var openFileCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFileCommand.ExecuteRequested += async (_, _) => await this.PickABFilesAsync();
            this.OpenFiles.Command = openFileCommand;

            var openFolderCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFolderCommand.ExecuteRequested += OpenFolderCommandOnExecuteRequested;
            this.OpenFolder.Command = openFolderCommand;

            var ejectCommand = new StandardUICommand(StandardUICommandKind.Delete);
            ejectCommand.ExecuteRequested += (_, _) => this.ViewModel.EjectFiles(this.LoadedList.SelectedItems);
            this.EjectItems.Command = ejectCommand;

            #endregion
        }

        private async void OpenFolderCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) {
            this.OpenFolder.IsEnabled = false;

            var picker = new FolderPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var folder = await picker.PickSingleFolderAsync();
            this.OpenFolder.IsEnabled = true;

            if (folder == null)
                return;

            _ = ViewModel.LoadAssetFolderAsync(folder).ConfigureAwait(false);
        }


        private async Task PickAPKFilesAsync() {
            this.OpenFiles.IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".apk");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var apkFile = await picker.PickSingleFileAsync();
            this.OpenFiles.IsEnabled = true;

            if (apkFile == null)
                return;

        }

        private async Task PickABFilesAsync() {
            this.OpenFiles.IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".ab");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var abFile = await picker.PickMultipleFilesAsync();
            this.OpenFiles.IsEnabled = true;

            if (abFile.Count == 0)
                return;

            await ViewModel.LoadAssetFilesAsync(abFile);
        }

        private void LoadAPKButton_OnClick(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
