namespace MaintManager.MAUI.Views.Inventory;
using MaintManager.MAUI.ViewModels.Inventory;

public partial class LotCreatePage : ContentPage, IQueryAttributable
{
    private readonly LotCreateViewModel _viewModel;

    public LotCreatePage(LotCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        _viewModel.ApplyQueryAttributes(query);

    protected override void OnAppearing() =>
        _viewModel.LoadMaterialsCommand.Execute(null);

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
