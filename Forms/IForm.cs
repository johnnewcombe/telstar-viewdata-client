using System.Collections.Generic;

namespace TelstarClient.Forms;

public interface IForm {
    public string ToString();
    public List<Field> Fields { set; get; }
    public Field GetCurrentField();
    public bool Next();
    public bool Previous();
}

public abstract class FormBase : IForm
{
    public abstract string ToString();
    public abstract List<Field> Fields { set; get; }
    public abstract Field GetCurrentField();
    public abstract bool Next();
    public abstract bool Previous();
} 