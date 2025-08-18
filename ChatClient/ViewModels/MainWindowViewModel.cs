using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ChatApp.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

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
    
    // Current text that is in the TextField
    [ObservableProperty] private string _currentMessage;

    private bool TryToConnect()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(ServerIp))
        {
            return false;
        }
        
        return true;
    }
    
    [RelayCommand]
    private void ConnectToServer()
    {
        if (!TryToConnect()) return;
        
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