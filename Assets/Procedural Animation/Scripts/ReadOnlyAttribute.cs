using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{
    public string boolName;

    public ReadOnlyAttribute(string boolName)
    {
        this.boolName = boolName;
    }
}
