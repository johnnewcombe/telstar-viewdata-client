using System.Collections.Generic;
using System.Text;
using TelstarClient.Models;

namespace TelstarClient.Display;

public static class MainMenu {

	private const byte R = 0x41;
	private const byte G = 0x42;
	private const byte Y = 0x43;
	private const byte B = 0x44;
	private const byte M = 0x45;
	private const byte C = 0x46;
	private const byte W = 0x47;

	private const byte a = 0x51;
	private const byte g = 0x52;
	private const byte y = 0x53;
	private const byte b = 0x54;
	private const byte m = 0x55;
	private const byte c = 0x56;
	private const byte w = 0x57;

	private const byte esc = 0x1b;

	/// <summary>
	/// Returns the welcome logo in viewdata byte format.
	/// </summary>
	/// <returns></returns>
	public static string GetLogo() {
		
		var logo = new StringBuilder();
		logo.Append("\r\n\n\n");
		logo.Append(Converters.ConvertFromMarkup("         [n][B][D]Viewdata[n][Y]Terminal[N][k](c) GlassTTY 2025\r\n\r\n\r\n\r\n"));
		logo.Append(Converters.ConvertFromMarkup("                   [G]T\r\n"));
		logo.Append(Converters.ConvertFromMarkup("                 [G]T[R]E[C]L\r\n"));
		logo.Append(Converters.ConvertFromMarkup("               [G]T[R]E[C]L[B]S[W]T\r\n"));
		logo.Append(Converters.ConvertFromMarkup("             [G]T[R]E[C]L[B]S[W]T[M]A[Y]R\r\n"));
		logo.Append(Converters.ConvertFromMarkup("               [C]L[B]S[W]T[M]A[Y]R\r\n"));
		logo.Append(Converters.ConvertFromMarkup("                 [W]T[M]A[Y]R\r\n"));
		logo.Append(Converters.ConvertFromMarkup("                   [Y]R\r\n"));
		
		return logo.ToString();
		
		// TODO use markup?? or edit.tf format.
		// i.e. define a string and convert it to bytes
		//var row = new List<byte>(){esc,G,(byte)'T'};

	}
}