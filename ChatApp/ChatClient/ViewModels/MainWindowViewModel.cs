using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ChatApp.Audio;
using ChatApp.Models;
using ChatApp.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    // Properties
    [ObservableProperty] private string _username = "Mads";
    [ObservableProperty] private string _serverIp = "127.0.0.1";
    [ObservableProperty] private Server.ConnectionStatus _connectionStatus;
    
    // Not the actual server, but more of a "manager" - It connects the client and sets up the packages
    private Server _server;
    
    // Current text that is in the TextField
    [ObservableProperty] private string? _currentMessage;
    
    // Audio class
    private ClientAudioHandler _audio;
    
    // Clients that are connected to the server
    [ObservableProperty] private ObservableCollection<ClientModel> _connectedClients;
    
    // All the messages that are sent on the server - used for displaying them in the UI
    public ObservableCollection<string> Messages { get; set; }

    public MainWindowViewModel()
    {
        _server = new Server();
        _audio = new ClientAudioHandler(_server);

        Messages = [];
        _connectedClients = [];
        
        // Events
        _server.MsgReceivedEvent += MessageReceived;
        _server.ConnectedEvent += ServerOnConnected;
        _server.DisconnectedEvent += ServerOnDisconnected;
        _server.StatusChanged += ServerOnStatusChanged;
    }

    private void ServerOnStatusChanged(Server.ConnectionStatus obj)
    {
        Dispatcher.UIThread.Post(() => ConnectionStatus = obj);
    }

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
    
    private void ServerOnDisconnected()
    {
        var guid = _server.PacketReader.ReadMessage();
        var client = ConnectedClients.Where(x => x.Guid == guid).FirstOrDefault();
        
        Dispatcher.UIThread.Post(() => ConnectedClients.Remove(client));
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
}