using UnityEngine;
using TMPro;
using System.Text;

public class VRDebugConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int maxLines = 12;

    private StringBuilder buffer = new StringBuilder();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        buffer.AppendLine(message);

        // Limite le nombre de lignes
        var lines = buffer.ToString().Split('\n');
        if (lines.Length > maxLines)
        {
            buffer.Clear();
            for (int i = lines.Length - maxLines; i < lines.Length; i++)
                buffer.AppendLine(lines[i]);
        }

        text.text = buffer.ToString();
    }
}