/*
    Copyright (c) 2025 John Newcombe
   
    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

*/

using System.Text;

namespace TelstarClient.Display;

public static class Menus {

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
		logo.Append(Converters.ConvertFromMarkup("[D][n][B][9]Viewdata Terminal\r\n"));

		// alignment guide
		//logo.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));

		return logo.ToString();
	}

	public static string GetMenu() {
		var menu = new StringBuilder();
		//menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
		menu.Append("\r\n");
		menu.Append(Converters.ConvertFromMarkup("[7][7][C][D]DIRECTORY\r\n\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[2][C]DIR NAME\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[0][PLACEHOLDER]"));
		menu.Append(Converters.ConvertFromMarkup("\r\n\n[4][C]Select '0' for Manual Dialling"));
		menu.Append(Converters.ConvertFromMarkup("\r\n\n[7][W]Ctrl-H for to view Help"));
		return menu.ToString();
	}

	public static string GetHelp() {
		
		var menu = new StringBuilder();
		menu.Append("\r\n");
		menu.Append(Converters.ConvertFromMarkup("[9][8][D]HELP\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n"));
		menu.Append(Converters.ConvertFromMarkup("[M]Ctrl C[C]Conceal\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[M]Ctrl R[C]Reveal\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[M]Ctrl H[C]Help\r\n\n"));
		//menu.Append(Converters.ConvertFromMarkup(""));
		//menu.Append(Converters.ConvertFromMarkup(""));
		menu.Append(Converters.ConvertFromMarkup("[M]Ctrl X[C]Disconnect\r\n\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[9]Press any key to Return"));
		//menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
		return menu.ToString();
	}

	public static string GetAbout() {
		var menu = new StringBuilder();
		menu.Append("\r\n");
		menu.Append(Converters.ConvertFromMarkup("[9][8][D]ABOUT\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[3]Version 0.1 (c) John Newcombe 2025\r\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[2][C]GlassTTY Viewdata Client is free\r\n"));
		menu.Append(Converters.ConvertFromMarkup("[2][C]software provided under the GPL 3\r\n"));
		menu.Append(Converters.ConvertFromMarkup("[2][C]CopyLeft licence.\r\n\n\n"));
		menu.Append(Converters.ConvertFromMarkup("[3]Press Escape to Return to Terminal"));
		//menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
		return menu.ToString();
	}
}
