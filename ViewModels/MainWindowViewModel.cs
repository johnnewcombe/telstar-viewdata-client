using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.AccessControl;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    private Display _display;
    private Comms.NetClient _client;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        _display = new Display();
        _client = new Comms.NetClient();

        
    }

    public void Connect()
    {
        _client.Connect("glasstty.com", 6502);
    }
    public void Disconnect()
    {
        _client.Disconnect();
    }
    

    public Display DisplayData
    {
        set
        {
            _display = value;
            OnPropertyChanged(nameof(DisplayData));
        }
        get
        {
            return _display;
        }
    }

    // implmentation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }
}