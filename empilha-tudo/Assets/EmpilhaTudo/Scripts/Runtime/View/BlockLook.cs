using System.Collections.Generic;
using Suricatus.FastDrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>
    /// As artes da partida, da pasta jogo/ do cliente: peças (peca*.png), base, brilho do encaixe perfeito
    /// e indicação de toque. Sem peça ou sem base, o jogo desenha com as cores do tema.
    /// </summary>
    public sealed class GameArts
    {
        public const string PieceName = "peca";
        public const string BaseName = "base";
        public const string SparkleName = "brilho";
        public const string TapName = "toque";

        public GameArts(ClientBundle bundle)
        {
            Pieces = bundle != null ? bundle.ArtsNamed(PieceName) : new List<Texture2D>();
            Base = bundle != null ? bundle.FindArt(BaseName) : null;
            Sparkle = bundle != null ? bundle.FindArt(SparkleName) : null;
            Tap = bundle != null ? bundle.FindArt(TapName) : null;
        }

        public IReadOnlyList<Texture2D> Pieces { get; }
        public Texture2D Base { get; }
        public Texture2D Sparkle { get; }
        public Texture2D Tap { get; }

        public Texture2D Piece(int variant) => Pieces.Count == 0 ? null : Pieces[(variant % Pieces.Count + Pieces.Count) % Pieces.Count];
    }

    /// <summary>
    /// O desenho das peças e da base. A arte da peça estica só no meio: as pontas (um quarto da altura de cada lado)
    /// ficam como estão, para os cantos e o brilho não deformarem em peças largas ou estreitas.
    /// </summary>
    public static class BlockLook
    {
        /// <summary>Parte de cima da arte da peça que é o corpo; o resto, embaixo, é sombra.</summary>
        public const float BodyShare = 0.86f;
        const float FallbackRadius = 14f;
        const float FallbackBorder = 5f;

        public static VisualElement Piece(UiFactory ui, GameArts arts, int variant)
        {
            var element = Div("et-block");
            var art = arts.Piece(variant);
            if (art != null)
            {
                element.style.backgroundImage = new StyleBackground(art);
                int cap = Mathf.RoundToInt(art.height / 4f);
                element.style.unitySliceLeft = cap;
                element.style.unitySliceRight = cap;
                element.style.unitySliceTop = 0;
                element.style.unitySliceBottom = 0;
                element.userData = art;
            }
            else
            {
                // Sem arte: um bloco nas cores do tema, ocupando só o corpo (sem sombra).
                var body = Div("et-block__body");
                var color = Palette(ui.Theme)[(variant % 4 + 4) % 4];
                body.style.backgroundColor = color;
                Border(body, UiFactory.Darken(color, 0.45f));
                element.Add(body);
            }
            return element;
        }

        public static VisualElement Base(UiFactory ui, GameArts arts)
        {
            var element = Div("et-base");
            if (arts.Base != null)
            {
                element.style.backgroundImage = new StyleBackground(arts.Base);
            }
            else
            {
                element.style.backgroundColor = ui.Theme.Secondary;
                Border(element, UiFactory.Darken(ui.Theme.Secondary, 0.5f));
            }
            return element;
        }

        /// <summary>Proporção altura ÷ largura da base. Sem arte, uma laje baixa.</summary>
        public static float BaseAspect(GameArts arts) => arts.Base != null && arts.Base.width > 0 ? (float)arts.Base.height / arts.Base.width : 0.2f;

        /// <summary>Ajusta as pontas da arte à altura em que a peça aparece, em px da interface.</summary>
        public static void FitSlices(VisualElement element, float heightPx)
        {
            if (element.userData is Texture2D art && art.height > 0 && heightPx > 0)
                element.style.unitySliceScale = heightPx / art.height;
        }

        static Color[] Palette(ThemeData theme) => new[]
        {
            theme.Primary,
            theme.Success,
            ThemeData.Blend(theme.Primary, theme.Error, 0.6f),
            ThemeData.Blend(theme.Success, theme.Secondary, 0.5f),
        };

        static void Border(VisualElement element, Color color)
        {
            UiFactory.Radius(element, FallbackRadius);
            element.style.borderTopWidth = FallbackBorder;
            element.style.borderRightWidth = FallbackBorder;
            element.style.borderBottomWidth = FallbackBorder;
            element.style.borderLeftWidth = FallbackBorder;
            element.style.borderTopColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
        }

        static VisualElement Div(string className)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            element.AddToClassList(className);
            return element;
        }
    }
}
