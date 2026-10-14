using System;
using System.Collections.Generic;
using Suricatus.FastDrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>
    /// A área do jogo: a base, os andares e a peça que se move. Um toque em qualquer lugar da área vai para
    /// <c>onTap</c>. Quando a torre cresce, tudo desce para a peça que se move ficar sempre à vista.
    /// Posições e tamanhos em fração do lado da área, que é quadrada.
    /// </summary>
    public sealed class TowerView : VisualElement
    {
        /// <summary>Altura de um andar (o corpo da peça).</summary>
        public const float Step = 0.09f;
        /// <summary>Altura da arte da peça, com a sombra embaixo.</summary>
        public const float ArtHeight = Step / BlockLook.BodyShare;
        /// <summary>Pé da base no começo da partida: um pouco abaixo da área, para sobrar espaço para a torre.</summary>
        const float BaseBottom = -0.07f;
        /// <summary>Altura máxima do pé da peça que se move; acima disso, a torre desce.</summary>
        const float FocusHeight = 0.62f;
        const float SparkleSize = 0.1f;
        const float HintSize = 0.15f;

        const int NextFrameMs = 16;
        const int PopMs = 120;
        const int CutFallMs = 700;
        const int SparkleMs = 520;
        const int FloatMs = 800;
        const int HintPulseMs = 450;
        const int CollapseStaggerMs = 35;
        const int CollapseStaggerMax = 10;

        readonly UiFactory ui;
        readonly Texts texts;
        readonly GameArts arts;
        readonly StackMatch state;
        readonly Action onTap;
        readonly VisualElement field;
        readonly VisualElement world;
        readonly VisualElement blocks;
        readonly VisualElement effects;
        readonly List<VisualElement> floors = new List<VisualElement>();
        readonly float baseTop;
        VisualElement moving;
        VisualElement hint;
        float fieldPx;
        bool locked;

        public TowerView(UiFactory ui, Texts texts, GameArts arts, StackMatch state, Action onTap)
        {
            this.ui = ui;
            this.texts = texts;
            this.arts = arts;
            this.state = state;
            this.onTap = onTap;

            AddToClassList("et-arena");
            ui.StyleCard(this);
            // O toque vale na área inteira, inclusive na borda do cartão.
            RegisterCallback<PointerDownEvent>(_ =>
            {
                if (!locked) this.onTap?.Invoke();
            });

            field = Div("et-field");
            Add(field);
            world = Div("et-world");
            field.Add(world);
            // Duas camadas que descem juntas: as peças e, sempre na frente delas, os efeitos.
            blocks = Div("et-layer");
            world.Add(blocks);
            effects = Div("et-layer");
            world.Add(effects);
            field.RegisterCallback<GeometryChangedEvent>(OnFieldResized);

            var start = state.Base;
            var platform = BlockLook.Base(ui, arts);
            float baseHeight = start.Width * BlockLook.BaseAspect(arts);
            Place(platform, start.Left, start.Width, BaseBottom, baseHeight);
            blocks.Add(platform);
            baseTop = BaseBottom + baseHeight;
        }

        /// <summary>Uma peça nova começou a se mover.</summary>
        public void ShowMoving(Block block)
        {
            moving = BlockLook.Piece(ui, arts, block.Variant);
            Position(moving, block);
            blocks.Add(moving);
            ScrollTo(block.Floor);
            if (block.Floor == 1) ShowHint(block);
        }

        /// <summary>Acompanha a peça que se move. Chamado a cada quadro.</summary>
        public void Tick()
        {
            var block = state.Moving;
            if (moving != null && block != null) moving.style.left = Length.Percent(block.Left * 100);
        }

        /// <summary>A peça encaixou: ela vira um andar, a parte sem apoio cai e o encaixe perfeito brilha.</summary>
        public void Place(DropResult drop, int perfectStreak)
        {
            HideHint();
            var element = moving;
            moving = null;
            if (element == null) return;
            Position(element, drop.Placed);
            floors.Add(element);
            if (drop.Fallen != null) DropCut(drop.Dropped, drop.Fallen);
            if (drop.Outcome == DropOutcome.Perfect) Celebrate(element, drop.Placed, perfectStreak);
        }

        /// <summary>A peça ficou sem apoio: ela cai inteira e a torre desaba de cima para baixo.</summary>
        public void Collapse(DropResult drop)
        {
            locked = true;
            HideHint();
            var top = state.Top;
            if (moving != null) Fall(moving, drop.Dropped.Center >= top.Center ? 1 : -1, 0, 50);
            moving = null;
            for (int i = floors.Count - 1, n = 0; i >= 0; i--, n++)
                Fall(floors[i], n % 2 == 0 ? 1 : -1, Math.Min(n, CollapseStaggerMax) * CollapseStaggerMs, 16 + n * 37 % 30);
        }

        /// <summary>Fim do tempo: a peça que se movia some e a área não aceita mais toques.</summary>
        public void Freeze()
        {
            locked = true;
            HideHint();
            moving?.AddToClassList("et-block--gone");
        }

        void OnFieldResized(GeometryChangedEvent evt)
        {
            fieldPx = evt.newRect.height;
            float pieceHeight = ArtHeight * fieldPx;
            foreach (var element in floors) BlockLook.FitSlices(element, pieceHeight);
            if (moving != null) BlockLook.FitSlices(moving, pieceHeight);
        }

        /// <summary>Pé do corpo da peça de um andar (o primeiro andar fica sobre a base).</summary>
        float BodyBottom(int floor) => baseTop + (floor - 1) * Step;

        void Position(VisualElement element, Block block)
        {
            Place(element, block.Left, block.Width, BodyBottom(block.Floor) - (ArtHeight - Step), ArtHeight);
            BlockLook.FitSlices(element, ArtHeight * fieldPx);
        }

        static void Place(VisualElement element, float left, float width, float bottom, float height)
        {
            element.style.left = Length.Percent(left * 100);
            element.style.width = Length.Percent(width * 100);
            element.style.bottom = Length.Percent(bottom * 100);
            element.style.height = Length.Percent(height * 100);
        }

        /// <summary>Desce a torre para o pé da peça que se move não passar do <see cref="FocusHeight"/>.</summary>
        void ScrollTo(int floor)
        {
            float offset = Mathf.Max(0, BodyBottom(floor) - FocusHeight);
            world.style.translate = new Translate(0, Length.Percent(offset * 100));
        }

        /// <summary>A parte cortada: a arte da peça inteira, recortada só no pedaço que ficou sem apoio.</summary>
        void DropCut(Block dropped, Block fallen)
        {
            if (fallen.Width <= 0) return;
            var clip = Div("et-cut");
            Place(clip, fallen.Left, fallen.Width, BodyBottom(fallen.Floor) - (ArtHeight - Step), ArtHeight);
            var piece = BlockLook.Piece(ui, arts, dropped.Variant);
            piece.style.left = Length.Percent((dropped.Left - fallen.Left) / fallen.Width * 100);
            piece.style.width = Length.Percent(dropped.Width / fallen.Width * 100);
            piece.style.bottom = 0;
            piece.style.height = Length.Percent(100);
            BlockLook.FitSlices(piece, ArtHeight * fieldPx);
            clip.Add(piece);
            blocks.Add(clip);

            int side = fallen.Center > dropped.Center ? 1 : -1;
            clip.schedule.Execute(() =>
            {
                clip.style.translate = new Translate(side * fieldPx * 0.08f, fieldPx * 0.75f);
                clip.style.rotate = new Rotate(side * 35f);
                clip.style.opacity = 0;
            }).StartingIn(NextFrameMs);
            clip.schedule.Execute(clip.RemoveFromHierarchy).StartingIn(CutFallMs + 100);
        }

        void Celebrate(VisualElement element, Block placed, int streak)
        {
            element.AddToClassList("et-block--pop");
            element.schedule.Execute(() => element.RemoveFromClassList("et-block--pop")).StartingIn(PopMs);

            float top = BodyBottom(placed.Floor) + Step;
            if (arts.Sparkle != null)
            {
                foreach (var x in new[] { placed.Left, placed.Right })
                {
                    var sparkle = Div("et-sparkle");
                    sparkle.style.backgroundImage = new StyleBackground(arts.Sparkle);
                    Place(sparkle, x - SparkleSize / 2, SparkleSize, top - SparkleSize / 2, SparkleSize);
                    effects.Add(sparkle);
                    sparkle.schedule.Execute(() => sparkle.AddToClassList("et-sparkle--out")).StartingIn(NextFrameMs);
                    sparkle.schedule.Execute(sparkle.RemoveFromHierarchy).StartingIn(SparkleMs);
                }
            }

            string text = texts.Get("empilha.perfeito", "Perfeito!");
            if (streak > 1) text += " ×" + streak;
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("et-float");
            ui.TitleFont(label);
            label.style.color = ui.Theme.HighlightOnCard;
            // Acima de onde a próxima peça aparece, para ela não cobrir o texto.
            label.style.left = Length.Percent(placed.Center * 100);
            label.style.bottom = Length.Percent((top + Step + 0.02f) * 100);
            effects.Add(label);
            label.schedule.Execute(() => label.AddToClassList("et-float--up")).StartingIn(NextFrameMs);
            label.schedule.Execute(label.RemoveFromHierarchy).StartingIn(FloatMs);
        }

        void Fall(VisualElement element, int side, int delayMs, float angle)
        {
            element.AddToClassList("et-block--falling");
            element.schedule.Execute(() =>
            {
                element.style.translate = new Translate(side * fieldPx * 0.3f, fieldPx * 1.2f);
                element.style.rotate = new Rotate(side * angle);
                element.style.opacity = 0;
            }).StartingIn(NextFrameMs + delayMs);
        }

        /// <summary>A mão de "toque aqui" acima da primeira peça, até o primeiro toque.</summary>
        void ShowHint(Block block)
        {
            if (arts.Tap == null || hint != null) return;
            hint = Div("et-hint");
            hint.style.backgroundImage = new StyleBackground(arts.Tap);
            Place(hint, 0.5f - HintSize / 2, HintSize, BodyBottom(block.Floor) + Step + 0.04f, HintSize);
            effects.Add(hint);
            hint.schedule.Execute(() => hint?.ToggleInClassList("et-hint--pulse")).Every(HintPulseMs);
        }

        void HideHint()
        {
            hint?.RemoveFromHierarchy();
            hint = null;
        }

        static VisualElement Div(string className)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            element.AddToClassList(className);
            return element;
        }
    }
}
