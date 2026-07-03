namespace TelstarClient.Forms;

public interface IForm {
    public string ToString();
    //public List<Field> Fields { set; get; }
    public Field GetCurrentField();
    public Field GetFieldById(string id);

    public int GetCursor();
    public bool IsValid();
    public bool ProcessFormKey(int asciiValue);
    Configuration.IConnection Connection { get; }
}

