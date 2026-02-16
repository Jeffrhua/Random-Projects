using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Cecs475.BoardGames.Chess.AvaloniaView;
using Cecs475.BoardGames.Othello.AvaloniaView;
using Cecs475.BoardGames.TicTacToe.AvaloniaView;

namespace Cecs475.BoardGames.AvaloniaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new GameChoiceWindow();//new MainWindow(new TicTacToeGameFactory());
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView() {
                GameFactory = new ChessGameFactory()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
