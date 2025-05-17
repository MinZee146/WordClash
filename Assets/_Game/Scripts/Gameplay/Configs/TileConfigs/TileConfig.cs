using UnityEngine;

[CreateAssetMenu(fileName = "TileConfig", menuName = "TileStats")]
public class TileConfig : ScriptableObject
{
    public char Letter;
    public int Score;
    public Color Color => GetColor();

    private Color GetColor()
    {
        return Letter switch
        {
            'A' or 'E' or 'I' or 'O' or 'U' or 'L' or 'N' or 'S' or 'T' or 'R' => Colors.FromHex("FFF6E9"),
            'D' or 'G' => Colors.FromHex("FFF100"),
            'B' or 'C' or 'M' or 'P' => Colors.FromHex("73EC8B"),
            'F' or 'H' or 'V' or 'W' or 'Y' => Colors.FromHex("FC8F54"),
            'K' => Colors.FromHex("ED254E"),
            'J' or 'X' => Colors.FromHex("4CC9FE"),
            'Q' or 'Z' => Colors.FromHex("8B5DFF"),
            _ => new Color(0.0f, 0.0f, 0.0f)
        };

    }
}