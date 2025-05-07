using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;
using TelstarClient.Models;

namespace TelstarClient.Display;

public static class MainMenu {

	/// <summary>
	/// Returns the welcome logo in viewdata byte format.
	/// </summary>
	/// <returns></returns>
	public static string GetLogo() {
		// https://edit.tf/#0:QIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpBw4cEHBAgQYOHBgg4cOCDBw4MCvDhw4YOHDgw4IEHBAgLGtTdGrS_0CBB_Ro_-pujRpf6NGrKo9bdGiR_0aH-gQf0CAsa1NVW9r_QIEH9ev_ol6_e0Vr1_0qg1NUCBB_QIES9f_QICxpV9-fkv_58af0CD-q-fPyXb8-PyqDU1QIEH9Ag2_Pj9AgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQICxpAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgLGkCBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECAsaQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECBAgQIECA
		var logo = new StringBuilder();

		// a quirky but efficient way to display the GlassTTY logo
		logo.Append("\r\n\n\n\n\n\n");
		logo.Append(Converters.ConvertFromMarkup("[w]///////////////////////////////////////\r\n\n"));
		logo.Append(Converters.ConvertFromMarkup("[c][sg] ppp p   `pp0 ppp `pp0 pppp`ppp0p  p\r\n"));
		logo.Append(
			Converters.ConvertFromMarkup("[c][sg]j7#+%\x7f   \x7f##\x7fj7##%\x7f##+ #k7#\"#\x7f#!\x7f  \x7f\r\n"));
		logo.Append(Converters.ConvertFromMarkup("[c][sg]j5*o5\x7f   \x7f//\x7f\"//o4+//}  j5   \x7f  \"//\x7f\r\n"));
		logo.Append(Converters.ConvertFromMarkup("[c][sg]*}|~%\x7f||4\x7f  \x7f*||~%m||?  j5   \x7f  m||?\r\n"));
		logo.Append(Converters.ConvertFromMarkup("\r\n\n\n"));
		logo.Append(Converters.ConvertFromMarkup("[D][n][B]         Viewdata Terminal\r\n"));

		// alignment guide
		//logo.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));

		return logo.ToString();
	}

	public static string GetMenu() {
		var menu = new StringBuilder();
		//menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
		menu.Append("\r\n");
		menu.Append(Converters.ConvertFromMarkup("              [C][D]DIRECTORY\r\n\n\n"));
		menu.Append(Converters.ConvertFromMarkup("  [C]DIR NAME\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[PLACEHOLDER]"));
		menu.Append(Converters.ConvertFromMarkup("\r\n\n[C]Select item or '0' for Manual Dialling"));
		return menu.ToString();
	}

	public static string GetHelp() {
		
		var menu = new StringBuilder();
		//menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
		menu.Append("\r\n");
		menu.Append(Converters.ConvertFromMarkup("                [C][D]HELP\r\n\n\n"));
		return menu.ToString();
	}
}
