using MaintManager.MAUI.ViewModels.Profile;

namespace MaintManager.MAUI.Views.Profile;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
