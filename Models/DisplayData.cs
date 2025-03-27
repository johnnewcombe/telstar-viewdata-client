namespace AvaloniaApplication1.Models;

public class DisplayData
{
    public int[] GetDisplayData()
    {
        int[] displayData = new int[Globals.COLS * Globals.ROWS];

        // TODO remove temp code to add some valid data
        for (int i = 0; i < Globals.COLS * Globals.ROWS; i++)
        {
            displayData[i] = 0xe276;
        }

        return displayData;
    }
}