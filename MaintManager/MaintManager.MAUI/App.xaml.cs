namespace MaintManager.MAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Página extremadamente simple: solo una etiqueta
        MainPage = new ContentPage
        {
            Content = new Label
            {
                Text = "✅ MaintManager MAUI está funcionando",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 24,
                TextColor = Colors.Black
            },
            BackgroundColor = Colors.White
        };
    }
}