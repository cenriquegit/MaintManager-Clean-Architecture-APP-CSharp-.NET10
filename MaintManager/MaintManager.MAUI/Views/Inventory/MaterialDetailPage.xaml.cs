using MaintManager.MAUI.ViewModels.Inventory;

namespace MaintManager.MAUI.Views.Inventory;

public partial class MaterialDetailPage : ContentPage
{
    private readonly MaterialDetailViewModel _viewModel;

    public MaterialDetailPage(MaterialDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing() => _viewModel.OnAppearing();
}
