using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ChatApp.Models;
using ChatApp.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    private Server _server;
    
    [ObservableProperty]
    private ObservableCollection<ClientModel> _connectedClients;
    public ObservableCollection<string> Messages { get; set; }

    public MainWindowViewModel()
    {
        _server = new Server();

        Messages = [];
        _connectedClients = [];
        
        // Events
        _server.MsgReceivedEvent += MessageReceived;
        _server.ConnectedEvent += ServerOnConnected;
        _server.DisconnectedEvent += ServerOnDisconnected;
        
    }

    private void ServerOnDisconnected()
    {
        var guid = _server.PacketReader.ReadMessage();
        var user = ConnectedClients.Where(x => x.Guid == guid).FirstOrDefault();
        
        Dispatcher.UIThread.Post(() => ConnectedClients.Remove(user));

    }


    private void ServerOnConnected()
    {
        var client = new ClientModel
        {
            Username = _server.PacketReader.ReadMessage(),
            Guid = _server.PacketReader.ReadMessage(),
        };
        
        Console.WriteLine($"Client {client.Username} was created");

        if (!ConnectedClients.Any(x => x.Guid == client.Guid))
        {
            
            Dispatcher.UIThread.Post(() => ConnectedClients.Add(client));
        }
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