using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

namespace TelstarClient.Display;

public static class Converters {

    # region Constants

    private const string HT = "\x09";
    private const string VT = "\x0B";
    private const string HOME = "\x1E";
    private const string CURON = "\x11";
    private const string CUROFF = "\x14";

    private const string SEPARATOR_GRAPHIC_DOTS_LOW = "000000000000000000000000000000000000000";
    private const string SEPARATOR_GRAPHIC_DOTS_MID = "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$";
    private const string SEPARATOR_GRAPHIC_DOTS_HIGH = "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!";
    private const string SEPARATOR_GRAPHIC_SOLID_LOW = "ppppppppppppppppppppppppppppppppppppppp";
    private const string SEPARATOR_GRAPHIC_SOLID_MID = ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";
    private const string SEPARATOR_GRAPHIC_SOLID_HIGH = "#######################################";
    private const string SEPARATOR_GRAPHIC_SOLID_DOUBLE = "sssssssssssssssssssssssssssssssssssssss";

    private const string ALPHA_BLACK = "\x1b\x40";
    private const string ALPHA_RED = "\x1b\x41";
    private const string ALPHA_GREEN = "\x1b\x42";
    private const string ALPHA_YELLOW = "\x1b\x43";
    private const string ALPHA_BLUE = "\x1b\x44";
    private const string ALPHA_MAGENTA = "\x1b\x45";
    private const string ALPHA_CYAN = "\x1b\x46";
    private const string ALPHA_WHITE = "\x1b\x47";
    private const string FLASH = "\x1b\x48";
    private const string STEADY = "\x1b\x49";
    private const string ENDBOX = "\x1b\x4a"; // Antiope only
    private const string STARTBOX = "\x1b\x4b"; // Antiope only
    private const string NORMAL_HEIGHT = "\x1b\x4c";
    private const string DOUBLE_HEIGHT = "\x1b\x4d";
    private const string DOUBLE_WIDTH = "\x1b\x4e"; // Antiope only
    private const string DOUBLE_SIZE = "\x1b\x4f"; // Antiope only
    private const string MOSAIC_BLACK = "\x1b\x50";
    private const string MOSAIC_RED = "\x1b\x51";
    private const string MOSAIC_GREEN = "\x1b\x52";
    private const string MOSAIC_YELLOW = "\x1b\x53";
    private const string MOSAIC_BLUE = "\x1b\x54";
    private const string MOSAIC_MAGENTA = "\x1b\x55";
    private const string MOSAIC_CYAN = "\x1b\x56";
    private const string MOSAIC_WHITE = "\x1b\x57";
    private const string CONCEAL = "\x1b\x58";
    private const string CONTIGUOS_GRAPHICS = "\x1b\x59"; 
    private const string SEPARATED_GRAPHICS = "\x1b\x5a"; 
    private const string CSI = "\x1b\x5b"; // Antiope only
    private const string END_BACKGROUND = "\x1b\x5c";
    private const string NEW_BACKGROUND = "\x1b\x5d";
    private const string HOLD_MOSAIC = "\x1b\x5e";
    private const string RELEASE_MOSAIC = "\x1b\x5f";

    #endregion
    
    /// <summary>
    /// Converts Markup to viewdata raw codes.
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    public static string ConvertFromMarkup(string markup) {

        markup = markup.Replace("[R]", ALPHA_RED);
        markup = markup.Replace("[R]", ALPHA_RED);
        markup = markup.Replace("[G]", ALPHA_GREEN);
        markup = markup.Replace("[Y]", ALPHA_YELLOW);
        markup = markup.Replace("[B]", ALPHA_BLUE);
        markup = markup.Replace("[M]", ALPHA_MAGENTA);
        markup = markup.Replace("[C]", ALPHA_CYAN);
        markup = markup.Replace("[W]", ALPHA_WHITE);
        markup = markup.Replace("[F]", FLASH);
        markup = markup.Replace("[S]", STEADY);
        markup = markup.Replace("[N]", NORMAL_HEIGHT);
        markup = markup.Replace("[D]", DOUBLE_HEIGHT);
        markup = markup.Replace("[-]", END_BACKGROUND);
        markup = markup.Replace("[k]", END_BACKGROUND);
        markup = markup.Replace("[n]", NEW_BACKGROUND);
        markup = markup.Replace("[r]", MOSAIC_RED);
        markup = markup.Replace("[g]", MOSAIC_GREEN);
        markup = markup.Replace("[y]", MOSAIC_YELLOW);
        markup = markup.Replace("[b]", MOSAIC_BLUE);
        markup = markup.Replace("[m]", MOSAIC_MAGENTA);
        markup = markup.Replace("[c]", MOSAIC_CYAN);
        markup = markup.Replace("[w]", MOSAIC_WHITE);
        markup = markup.Replace("[h.]", SEPARATOR_GRAPHIC_DOTS_HIGH);
        markup = markup.Replace("[m.]", SEPARATOR_GRAPHIC_DOTS_MID);
        markup = markup.Replace("[l.]", SEPARATOR_GRAPHIC_DOTS_LOW);
        markup = markup.Replace("[h-]", SEPARATOR_GRAPHIC_SOLID_HIGH);
        markup = markup.Replace("[m-]", SEPARATOR_GRAPHIC_SOLID_MID);
        markup = markup.Replace("[l-]", SEPARATOR_GRAPHIC_SOLID_LOW);
        markup = markup.Replace("[=]", SEPARATOR_GRAPHIC_SOLID_DOUBLE);
        markup = markup.Replace("[_+]", CURON);
        markup = markup.Replace("[_-]", CUROFF);
        markup = markup.Replace("[@]", HOME);
        markup = markup.Replace("[H]", HT);
        markup = markup.Replace("[V]", VT);
        markup = markup.Replace("[hg]", HOLD_MOSAIC);
        markup = markup.Replace("[rg]", RELEASE_MOSAIC);
        markup = markup.Replace("[sg]", SEPARATED_GRAPHICS);
        markup = markup.Replace("[cg]", CONTIGUOS_GRAPHICS);

        // TODO implement markup that includes spaces e.g. [12] = 12 space (0x20) characters
        //  any changes here need to be reflected in the GetMarkupLen() Util method
        //for n:=1; n<COLS;n++{
        //}

        return markup;

    }
}