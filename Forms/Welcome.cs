using System.Collections.Generic;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Welcome : IForm {

    public string ToString() {
        // https://edit.tf/#0:QIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpBw4cEHBAgQYOHBgg4cOCDBw4MCvDhw4YOHDgw4IEHBAgLGtTdGrS_0CBB_Ro_-pujRpf6NGrKo9bdGiR_0aH-gQf0CAsa1NVW9r_QIEH9ev_ol6_e0Vr1_0qg1NUCBB_QIES9f_QICxpV9-fkv_58af0CD-q-fPyXb8-PyqDU1QIEH9Ag2_Pj9AgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECA
        var logo = new StringBuilder();

        // a quirky but efficient way to display the GlassTTY logo
        // note the numbers in square brackets represent spase e.f.[10] means 10 spaces
        // fields that can be edited aare marked with curly braces e.g. {[10]} is a
        // field with 10 spaces

        logo.Append("\r\n\n\n\n\n\n");
        logo.Append(Converters.ConvertFromMarkup("[w]///////////////////////////////////////\r\n\n"));
        logo.Append(Converters.ConvertFromMarkup("[c][sg] ppp p   `pp0 ppp `pp0 pppp`ppp0p  p\r\n"));
        logo.Append(
            Converters.ConvertFromMarkup("[c][sg]j7#+%\x7f   \x7f##\x7fj7##%\x7f##+ #k7#\"#\x7f#!\x7f  \x7f\r\n"));
        logo.Append(Converters.ConvertFromMarkup("[c][sg]j5*o5\x7f   \x7f//\x7f\"//o4+//}  j5   \x7f  \"//\x7f\r\n"));
        logo.Append(Converters.ConvertFromMarkup("[c][sg]*}|~%\x7f||4\x7f  \x7f*||~%m||?  j5   \x7f  m||?\r\n"));
        logo.Append(Converters.ConvertFromMarkup("\r\n\n\n"));
        logo.Append(Converters.ConvertFromMarkup("[D][n][B][9]Viewdata Terminal"));


        // alignment guide
        //logo.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));

        return logo.ToString();
    }

    public List<Field> Fields { get; set; }

    public Field GetCurrentField() {
        throw new System.NotImplementedException();
    }

    public bool Next() {
        throw new System.NotImplementedException();
    }
    public bool Previous() {
        throw new System.NotImplementedException();
    }

}