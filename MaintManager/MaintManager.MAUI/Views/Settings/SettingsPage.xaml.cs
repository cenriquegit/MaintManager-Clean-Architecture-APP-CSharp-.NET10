// MaintManager.MAUI/Views/Settings/SettingsPage.xaml.cs
using MaintManager.MAUI.ViewModels.Settings;
namespace MaintManager.MAUI.Views.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
