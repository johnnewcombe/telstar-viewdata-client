using System.Collections.Generic;

namespace TelstarClient.Forms;

public interface IForm {
    public string ToString();
    public List<Field> Fields { set; get; }
    public Field GetCurrentField();
    public bool Next();
    public bool Previous();
}