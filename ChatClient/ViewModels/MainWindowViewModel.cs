using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Threading;
using ChatApp.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    private Server _server;

    public ObservableCollection<string> Messages { get; set; }

    public MainWindowViewModel()
    {
        _server = new Server();

        Messages = [];
        
        // Events
        _server.MsgReceivedEvent += MessageReceived;
    }
    
    // Properties
    [ObservableProperty] private string _username = "Mads";
    [ObservableProperty] private string _serverIp = "127.0.0.1";
    [ObservableProperty] private string _currentMessage;

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

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrEmpty(CurrentMessage)) return;
        
        Console.WriteLine($"Trying to send message - {CurrentMessage}");
        
        _server.SendMessageToServer(CurrentMessage);
        
        CurrentMessage = string.Empty;
    }

    private void MessageReceived()
    {
        var msg = _server.PacketReader.ReadMessage();
        Dispatcher.UIThread.Post(() => Messages.Add(msg));
    }
}