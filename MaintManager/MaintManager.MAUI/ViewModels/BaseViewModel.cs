using CommunityToolkit.Mvvm.ComponentModel;

namespace MaintManager.MAUI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _title = string.Empty;

    // Propiedad calculada: éxito cuando no está ocupado, no tiene error y no está vacío
    public bool IsSuccess => !IsBusy && !HasError && !IsEmpty;

    // Método helper para ejecutar operaciones asíncronas con manejo de estado
    protected async Task ExecuteAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
            OnPropertyChanged(nameof(IsSuccess));
        }
    }
}