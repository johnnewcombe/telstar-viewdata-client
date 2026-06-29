using System.Collections.Generic;
using System.Linq;
using TelstarClient.Configuration;
using TelstarClient.Display;

namespace TelstarClient.Forms;
public abstract class FormBase : IForm
{
    private int _currentField = 0;
    protected DisplayManager _displayManager;

    protected FormBase(DisplayManager displayManager, Configuration.Connection connection)
    {
        _displayManager = displayManager;
        Connection = connection;
    }

    public List<Field> Fields { get; set; } = new List<Field>();

    public void ProcessFormKey(int asciiValue)
    {
        
    }

    public Connection Connection { get; }

    public Field GetCurrentField()
    {
        return Fields[_currentField];
    }

    /// <summary>
    /// Get the fiels based on the 'id' property.
    /// </summary>
    /// <param id="name"></param>
    public Field GetFieldById(string id)
    {
        return Fields.FirstOrDefault(f => f.ID == id);
    }
    
    public bool Next()
    {
        if (_currentField < Fields.Count - 1)
        {
            _currentField++;
            return true;
        }
        return false;
    }

    public bool Previous()
    {
        if (_currentField > 0)
        {
            _currentField--;
            return true;
        }
        return false;
    }

    public bool IsValid()
    {
        // TODO: must check the values in each field to ensure they are valid for the field type
        //   and that all required fields are present.
        foreach (var field in Fields)
        {
            if (!field.IsValid())
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Forces derived classes to implement their own ToString
    public override string ToString()
    {
        foreach(var field in Fields)
        {
            return $"{field.ID}:{field.Value}";
        }
        return string.Empty;
    }
}