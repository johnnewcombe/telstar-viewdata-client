using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    private Display _display;
    

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        _display = new Display();
    }

    public void Test()
    {
        this.DisplayData = _display;
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