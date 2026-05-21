using MaintManager.MAUI.ViewModels.Inventory;

namespace MaintManager.MAUI.Views.Inventory;

public partial class CreateMaterialPage : ContentPage
{
    private readonly CreateMaterialViewModel _vm;

    public CreateMaterialPage(CreateMaterialViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Categories.Count == 0)
            await _vm.LoadCategoriesCommand.ExecuteAsync(null);
    }
}
