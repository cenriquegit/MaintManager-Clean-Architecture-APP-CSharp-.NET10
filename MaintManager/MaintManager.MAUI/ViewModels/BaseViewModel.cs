using System.Net.Http;
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
    private bool _isEmpty = true;

    [ObservableProperty]
    private string _title = string.Empty;

    public bool IsSuccess => !IsBusy && !HasError && !IsEmpty;

    protected async Task ExecuteAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            IsLoading = true;
            HasError = false;
            IsEmpty = false;
            ErrorMessage = string.Empty;

            var timeoutTask = Task.Delay(30000);
            var opTask = operation();
            var completed = await Task.WhenAny(opTask, timeoutTask);

            if (completed == timeoutTask)
            {
                HasError = true;
                ErrorMessage = "La operación tardó demasiado. Intenta nuevamente.";
            }
            else
            {
                await opTask;
            }
        }
        catch (HttpRequestException)
        {
            HasError = true;
            IsEmpty = false;
            ErrorMessage = "Error de conexión con el servidor. Verifica que la API esté en ejecución y que la URL sea correcta en Configuración.";
        }
        catch (TaskCanceledException)
        {
            HasError = true;
            IsEmpty = false;
            ErrorMessage = "La conexión tardó demasiado. Verifica tu conexión de red e intenta nuevamente.";
        }
        catch (Exception)
        {
            HasError = true;
            IsEmpty = false;
            ErrorMessage = "Ocurrió un error inesperado. Intenta nuevamente.";
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
            OnPropertyChanged(nameof(IsSuccess));
        }
    }
}