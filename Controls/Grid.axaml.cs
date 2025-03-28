using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication1.Controls;

public partial class Grid : UserControl
{
    public Grid()
    {
        InitializeComponent();
    }
    
    public List<int> DataSource{set; get;}
}