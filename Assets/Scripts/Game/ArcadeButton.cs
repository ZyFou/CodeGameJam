using UnityEngine;

public class ArcadeButton : MonoBehaviour, IClickable
{
    [Header("Visual")]
    [SerializeField] private Renderer rend;

    public int Index { get; private set; }

    BoardManager board;
    MaterialPropertyBlock mpb;

    public void Init(BoardManager boardManager, int index)
    {
        board = boardManager;
        Index = index;

        mpb ??= new MaterialPropertyBlock();

        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
    }

    public void Click(ClickContext ctx)
    {
        // Le Board décide si ce clic compte ou non
        board.OnButtonPressed(Index);
    }

    public void SetVisual(ButtonKind kind, bool isOn)
    {
        if (rend == null) return;
        mpb ??= new MaterialPropertyBlock();

        // Couleur selon kind
        Color c = kind switch
        {
            ButtonKind.Green  => new Color(0.2f, 1f, 0.2f),
            ButtonKind.Yellow => new Color(1f, 0.85f, 0.2f),
            ButtonKind.Black  => new Color(0.05f, 0.05f, 0.08f),
            _                 => new Color(0.6f, 0.6f, 0.6f) // gris
        };

        // OFF = pas d'emission
        float intensity = isOn ? 3.0f : 0.0f;

        rend.GetPropertyBlock(mpb);

        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_BaseColor"))
            mpb.SetColor("_BaseColor", c);
        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_Color"))
            mpb.SetColor("_Color", c);
        if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_EmissionColor"))
            mpb.SetColor("_EmissionColor", c * intensity);

        rend.SetPropertyBlock(mpb);
    }
}
