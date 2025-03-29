using System;
using System.ComponentModel;
using System.Diagnostics;
using AvaloniaApplication1.Comms;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Display _display;
    TCPClient _tcp;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        _display = new Display();
    }

    public void Connect()
    {
        //_client = new Comms.NetClient("glasstty.com", 6502);
        try
        {
            _tcp = new TCPClient("glasstty.com", 6502);
            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataRecievedEvent += OnRecieved;
            _tcp.Connect();
        }
        catch (Exception ex)
        {
            // Catch errors in Connection and Recieve Callbacks
            Debug.Print("Error : " + ex.ToString());
        }
    }

    // Connection Status Listner
    private static void OnConnect(bool status)
    {
        Debug.Print("Connection : " + status.ToString());
    }

    // Data Recieved Listner
    private static void OnRecieved(string data)
    {
        Debug.Print(data);
    }
    
    public void Disconnect()
    {
       if (_tcp.Write("*"))
       {
           Debug.Print("Data Sent");
       }
       
       
    }

    public Display DisplayData
    {
        set
        {
            _display = value;
            OnPropertyChanged(nameof(DisplayData));
        }
        get { return _display; }
    }

    // implmentation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }
}