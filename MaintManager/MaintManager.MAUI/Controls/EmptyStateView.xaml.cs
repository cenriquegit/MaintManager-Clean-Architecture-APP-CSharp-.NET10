using System.Windows.Input;

namespace MaintManager.MAUI.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyStateView), "📭",
            propertyChanged: OnContentChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyStateView), string.Empty,
            propertyChanged: OnContentChanged);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyStateView), string.Empty,
            propertyChanged: OnContentChanged);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(EmptyStateView), string.Empty,
            propertyChanged: OnActionTextChanged);

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(EmptyStateView));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public EmptyStateView()
    {
        InitializeComponent();
        UpdateContent();
    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyStateView view)
            view.UpdateContent();
    }

    private static void OnActionTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyStateView view)
            view.UpdateActionButton();
    }

    private void UpdateContent()
    {
        IconLabel.Text = Icon;
        TitleLabel.Text = Title;
        MessageLabel.Text = Message;
        UpdateActionButton();
    }

    private void UpdateActionButton()
    {
        ActionButton.Text = ActionText;
        ActionButton.IsVisible = !string.IsNullOrWhiteSpace(ActionText);
    }

    private void OnActionClicked(object? sender, EventArgs e)
    {
        if (ActionCommand?.CanExecute(null) == true)
            ActionCommand.Execute(null);
    }
}
