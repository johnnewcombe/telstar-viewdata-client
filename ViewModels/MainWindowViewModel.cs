using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private int[] displayData = new int[Globals.COLS*Globals.ROWS];
    
    public int[] DisplayData
    {
        get
        {
            
            /* Mode 7 font
             * Graphics start at e201
             * Non-contiguous graphics start at e2c1
             * Upper part of Alpha double height e021
             * Lower part of Alpha double height e121
             *
             */
            
            // temp code to add some valid data
            for (int i = 0; i < Globals.COLS * Globals.ROWS; i++)
            {
                displayData[i] = 0xe276;
            }
            
            return displayData;
        }
    }
    

}
