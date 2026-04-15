// MaintManager.MAUI/Controls/StateView.xaml.cs
namespace MaintManager.MAUI.Controls;

/// <summary>
/// Control reutilizable que encapsula los 4 estados obligatorios de toda vista con datos.
/// Estados: Loading → Error → Empty → Success.
/// El contenido de Success se define en el Content del control.
/// </summary>
public partial class StateView : ContentView
{
    // ── BindableProperties ────────────────────────────────────────────

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(StateView), false,
            propertyChanged: OnStateChanged);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(StateView), false,
            propertyChanged: OnStateChanged);

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(StateView), null,
            propertyChanged: OnStateChanged);

    public static readonly BindableProperty IsEmptyProperty =
        BindableProperty.Create(nameof(IsEmpty), typeof(bool), typeof(StateView), false,
            propertyChanged: OnStateChanged);

    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(System.Windows.Input.ICommand), typeof(StateView));

    public static readonly BindableProperty EmptyMessageProperty =
        BindableProperty.Create(nameof(EmptyMessage), typeof(string), typeof(StateView),
            "No hay datos disponibles.", propertyChanged: OnStateChanged);

    public static readonly BindableProperty EmptyIconProperty =
        BindableProperty.Create(nameof(EmptyIcon), typeof(string), typeof(StateView),
            "📋", propertyChanged: OnStateChanged);

    public static readonly BindableProperty LoadingMessageProperty =
        BindableProperty.Create(nameof(LoadingMessage), typeof(string), typeof(StateView),
            "Cargando...", propertyChanged: OnStateChanged);

    // ── Propiedades ───────────────────────────────────────────────────

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public System.Windows.Input.ICommand? RetryCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public string EmptyMessage
    {
        get => (string)GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public string EmptyIcon
    {
        get => (string)GetValue(EmptyIconProperty);
        set => SetValue(EmptyIconProperty, value);
    }

    public string LoadingMessage
    {
        get => (string)GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    // ── Constructor ───────────────────────────────────────────────────

    public StateView()
    {
        InitializeComponent();
        RetryButton.Clicked += (_, _) => RetryCommand?.Execute(null);
    }

    // ── Lógica de estado ──────────────────────────────────────────────

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StateView sv) sv.UpdateState();
    }

    private void UpdateState()
    {
        // Actualizar textos
        LoadingLabel.Text = LoadingMessage;
        ErrorLabel.Text   = ErrorMessage ?? "Ocurrió un error. Por favor intente nuevamente.";
        EmptyLabel.Text   = EmptyMessage;
        EmptyIconLabel.Text = EmptyIcon;

        // Determinar estado activo (uno solo a la vez)
        var isSuccess = !IsLoading && !HasError && !IsEmpty;

        LoadingView.IsVisible   = IsLoading;
        LoadingIndicator.IsRunning = IsLoading;
        ErrorView.IsVisible     = !IsLoading && HasError;
        EmptyView.IsVisible     = !IsLoading && !HasError && IsEmpty;
        SuccessContent.IsVisible = isSuccess;
    }
}
