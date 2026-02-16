using Avalonia.Controls;
using Cecs475.BoardGames.AvaloniaView;

namespace Cecs475.BoardGames.AvaloniaApp;

public partial class MainWindow : Window
{
    public MainWindow(IAvaloniaGameFactory factory)
    {
        InitializeComponent();
        mMainView.GameFactory = factory;
        Title = $"Let's play {factory.GameName}!";
    }
}
