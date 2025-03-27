using System.Collections.ObjectModel;
using System.ComponentModel;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<Character> _characters;
    private int[] _displayData = new int[Globals.COLS * Globals.ROWS];
    private int _characterReceived;
    
    public ObservableCollection<Character> Characters
    {
        get { return _characters; }
        set { SetProperty(ref _characters, value); }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        const string defaultValue = "c";
        const int defaultAttribute = 0;

        Characters = new ObservableCollection<Character>();

        for (var i = 0; i < Globals.COLS * Globals.ROWS; i++)
        {
            Characters.Add(new Character { Value = defaultValue, Attribute = defaultAttribute });
        }
    }

    public void Test()
    {
        this.CharacterReceived = 0xe276;
    }
    
    public void ClearDisplay()
    {
        int[] dummyData = new int[Globals.COLS * Globals.ROWS];
        for (int i = 0; i < Globals.COLS * Globals.ROWS; i++)
        {
            dummyData[i] = 0x20;
        }
        this.DisplayData = dummyData;
    }

    public Cursor Cursor{set; get;}
    
    public int CharacterReceived {
        set
        {
            _characterReceived = value;
            OnPropertyChanged(nameof(CharacterReceived));
        }
        get
        {
            return _characterReceived;
        }
    }

    public int[] DisplayData
    {
        set
        {
            _displayData = value;
            OnPropertyChanged(nameof(DisplayData));
        }
        get
        {
            return _displayData;
        }
    }

    // implmentation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }
}