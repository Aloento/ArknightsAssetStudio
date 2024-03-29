namespace SoarCraft.QYun.ArknightsAssetStudio.Helpers {
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public class NavHelper {
        // This helper class allows to specify the page that will be shown when you click on a NavigationViewItem
        //
        // Usage in xaml:
        // <winui:NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavHelper.NavigateTo="AppName.ViewModels.MainViewModel" />
        //
        // Usage in code:
        // NavHelper.SetNavigateTo(navigationViewItem, typeof(MainViewModel).FullName);
        public static string GetNavigateTo(NavigationViewItem item) => (string)item.GetValue(NavigateToProperty);

        public static void SetNavigateTo(NavigationViewItem item, string value) =>
            item.SetValue(NavigateToProperty, value);

        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavHelper),
                new PropertyMetadata(null));
    }
}
