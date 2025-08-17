using System;
using ChatApp.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    private Server _server;

    public MainWindowViewModel()
    {
        _server = new Server();
    }

    [ObservableProperty]
    private string _username;
    
    [ObservableProperty] 
    private string _serverIp;

    [RelayCommand]
    private void ConnectToServer()
    {
        if (string.IsNullOrEmpty(Username))
        {
            Console.WriteLine("Cant connect without a username!!!");
        }
        
        Console.WriteLine($"Trying to connect to server... as {Username}");
        _server.ConnectToServer(ServerIp, 7891, Username);
    }
    
}