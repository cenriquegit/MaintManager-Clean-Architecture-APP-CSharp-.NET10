namespace MaintManager.MAUI.Controls;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusBadge), string.Empty,
            propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(StatusBadge), Colors.Gray,
            propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(string), typeof(StatusBadge), "Medium",
            propertyChanged: OnSizeChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Size
    {
        get => (string)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
        ApplySize();
    }

    private static void OnBindablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge)
            badge.ApplySize();
    }

    private void ApplySize()
    {
        switch (Size)
        {
            case "Small":
                BadgeLabel.FontSize = 11;
                BadgeBorder.Padding = new Thickness(6, 2);
                break;
            case "Large":
                BadgeLabel.FontSize = 15;
                BadgeBorder.Padding = new Thickness(14, 6);
                break;
            default:
                BadgeLabel.FontSize = 13;
                BadgeBorder.Padding = new Thickness(10, 4);
                break;
        }
    }
}
