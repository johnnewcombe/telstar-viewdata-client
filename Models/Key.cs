using System.Text;
using Avalonia.Input;

namespace TelstarClient.Models;

public class Key {
    public string Ascii { set; get; }
    public KeyModifiers KeyModifiers { set; get; }
}