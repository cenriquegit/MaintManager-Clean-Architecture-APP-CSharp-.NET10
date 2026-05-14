namespace MaintManager.MAUI.Controls;

public partial class KpiCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty TrendUpProperty =
        BindableProperty.Create(nameof(TrendUp), typeof(bool), typeof(KpiCard), true,
            propertyChanged: OnTrendChanged);

    public static readonly BindableProperty TrendValueProperty =
        BindableProperty.Create(nameof(TrendValue), typeof(string), typeof(KpiCard), string.Empty,
            propertyChanged: OnTrendChanged);

    public static readonly BindableProperty AccentColorProperty =
        BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(KpiCard), Color.FromArgb("#1565C0"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool TrendUp
    {
        get => (bool)GetValue(TrendUpProperty);
        set => SetValue(TrendUpProperty, value);
    }

    public string TrendValue
    {
        get => (string)GetValue(TrendValueProperty);
        set => SetValue(TrendValueProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public KpiCard()
    {
        InitializeComponent();
        UpdateTrend();
    }

    private static void OnTrendChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is KpiCard card)
            card.UpdateTrend();
    }

    private void UpdateTrend()
    {
        TrendIcon.Text = TrendUp ? "\u25B2" : "\u25BC";
        TrendIcon.TextColor = TrendUp ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828");
        TrendLabel.Text = TrendValue;
    }
}
