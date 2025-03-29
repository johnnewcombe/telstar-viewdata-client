using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Security.AccessControl;
using AvaloniaApplication1.Comms;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Display _display;
    private Comms.NetClient _client;
    AsynchronousClient tcp;
    
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
            tcp = new AsynchronousClient("46.101.66.218", int.Parse("6502"));
            tcp.OnConnectEvent += new AsynchronousClient.OnConnectEventHandler(OnConnect);
            tcp.OnDataRecievedEvent += new AsynchronousClient.DataReceivedEventHandler(OnRecieved);
            tcp.Connect();
        }
        catch (Exception ex)
        {
            // Catch errors in Connection and Recieve Callbacks
            Debug.Print("Error : " + ex.ToString());
        }
    }

    // Connection Status Listner
    private void OnConnect(bool status)
    {
        Debug.Print("Connection : " + status.ToString());
    }

    // Data Recieved Listner
    private void OnRecieved(string data)
    {
        Debug.Print(data);
    }
    
    public void Disconnect()
    {
       if (tcp.Write("*"))
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