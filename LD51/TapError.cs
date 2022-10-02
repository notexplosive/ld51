using Microsoft.Xna.Framework;

namespace LD51;

public record TapError(string Title, string Description) : ITapAction
{
    public void Execute()
    {
        LudumCartridge.Ui.ErrorToast.ShowError(Title);
    }

    public Color Color => Color.DarkRed;
}
