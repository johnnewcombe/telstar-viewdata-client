using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication1.Models;

public class Character : ObservableObject
{
    private string _value;
    private byte _attribute;


    public string Value
    {
        get{return _value;}
        set{SetProperty(ref _value, value);}
    }
    public byte Attribute
    {
        get{return _attribute;}
        set{SetProperty(ref _attribute, value);}
    }
}