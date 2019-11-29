using UnityEngine;
using GameplayIngredients.Actions;
using UnityEngine.UI;
public class SetButtonColorAction : ActionBase
{
    public Button button;
    public Color normalColor;
    public Color highlightedColor;
    public Color selectedColor;
    public Color pressedColor;
    public Color disabledColor;
    public override void Execute(GameObject instigator = null)
    {
        ColorBlock colors = new ColorBlock();
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.selectedColor = selectedColor;
        colors.pressedColor = pressedColor;
        colors.disabledColor = disabledColor;
        colors.colorMultiplier = button.colors.colorMultiplier;
        colors.fadeDuration = button.colors.fadeDuration;
        button.colors = colors;
    }
}