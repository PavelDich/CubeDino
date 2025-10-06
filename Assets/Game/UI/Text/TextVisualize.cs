using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class TextVisualize : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncText))]
    public string Text;
    public UnityEvent<string> OnTextVisible = new UnityEvent<string>();
    public void SyncText(string oldValue, string newValue)
    {
        OnTextVisible.Invoke(newValue);
    }
}
