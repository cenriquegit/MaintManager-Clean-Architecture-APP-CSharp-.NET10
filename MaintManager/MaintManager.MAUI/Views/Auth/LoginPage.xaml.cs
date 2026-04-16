using MaintManager.MAUI.ViewModels.Auth;

namespace MaintManager.MAUI.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}