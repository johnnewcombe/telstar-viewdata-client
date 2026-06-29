using System.Collections.Generic;

namespace TelstarClient.Forms;

public interface IForm {
    public string ToString();
    public List<Field> Fields { set; get; }
    public Field GetCurrentField();
    public Field GetFieldById(string id);
    public bool Next();
    public bool Previous();
    public bool IsValid();
    public bool ProcessFormKey(int asciiValue);
    Configuration.Connection Connection { get; }
}

