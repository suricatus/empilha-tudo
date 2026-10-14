using System;
using Suricatus.FastDrive;
using UnityEngine.UIElements;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>
    /// A área de apoio: a próxima faixa com a barra de progresso, os andares e os encaixes perfeitos, e a legenda
    /// do toque. Tocar aqui também solta a peça.
    /// </summary>
    public sealed class MatchPanelView : VisualElement
    {
        const int PopMs = 220;

        readonly Texts texts;
        readonly EmpilhaContent content;
        readonly Label goalCaption;
        readonly Label goalName;
        readonly VisualElement barFill;
        readonly Label floorsLabel;
        readonly Label perfectsLabel;
        Tier shownGoal;
        bool started;

        public MatchPanelView(UiFactory ui, Texts texts, GameArts arts, EmpilhaContent content, Action onTap)
        {
            this.texts = texts;
            this.content = content;
            AddToClassList("et-panel");
            ui.StyleCard(this);
            RegisterCallback<PointerDownEvent>(_ => onTap?.Invoke());

            var goal = Div("et-goal");
            goalCaption = ui.Text("", soft: true);
            goalCaption.AddToClassList("et-goal__caption");
            goal.Add(goalCaption);
            goalName = ui.Title("", onCard: true);
            goalName.AddToClassList("et-goal__name");
            goal.Add(goalName);
            var bar = Div("et-bar");
            bar.style.backgroundColor = ui.Theme.Secondary;
            barFill = Div("et-bar__fill");
            barFill.style.backgroundColor = ui.Theme.HighlightOnCard;
            bar.Add(barFill);
            goal.Add(bar);
            Add(goal);

            var stats = Div("et-stats");
            floorsLabel = Stat(ui, stats, texts.Get("empilha.andares", "Andares"));
            perfectsLabel = Stat(ui, stats, texts.Get("empilha.perfeitos", "Perfeitos"));
            Add(stats);

            var legend = Div("et-legend");
            if (arts.Tap != null)
            {
                var icon = Div("et-legend__icon");
                icon.style.backgroundImage = new StyleBackground(arts.Tap);
                legend.Add(icon);
            }
            var caption = ui.Text(texts.Get("empilha.toque", "Toque na tela para soltar a peça"));
            caption.AddToClassList("et-legend__label");
            legend.Add(caption);
            Add(legend);
        }

        public void SetProgress(int floors, int perfects, int score)
        {
            floorsLabel.text = floors.ToString();
            perfectsLabel.text = perfects.ToString();

            var current = content.TierFor(score);
            var next = content.NextTierAfter(score);
            float fill;
            if (next != null)
            {
                goalCaption.text = texts.Get("empilha.proximaMeta", "Próxima meta");
                goalName.text = next.Name;
                int from = current != null ? current.MinScore : 0;
                fill = next.MinScore > from ? (float)(score - from) / (next.MinScore - from) : 0;
            }
            else
            {
                goalCaption.text = texts.Get("empilha.metaFinal", "Faixa máxima!");
                goalName.text = current != null ? current.Name : "";
                fill = 1;
            }
            barFill.style.width = Length.Percent(Math.Max(0, Math.Min(1, fill)) * 100);

            var goal = next ?? current;
            if (started && goal != shownGoal)
            {
                goalName.AddToClassList("et-goal__name--pop");
                goalName.schedule.Execute(() => goalName.RemoveFromClassList("et-goal__name--pop")).StartingIn(PopMs);
            }
            shownGoal = goal;
            started = true;
        }

        static Label Stat(UiFactory ui, VisualElement parent, string caption)
        {
            var block = Div("et-stat");
            var value = ui.Title("0", onCard: true);
            value.AddToClassList("et-stat__value");
            block.Add(value);
            var label = ui.Text(caption, soft: true);
            label.AddToClassList("et-stat__label");
            block.Add(label);
            parent.Add(block);
            return value;
        }

        static VisualElement Div(string className)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            element.AddToClassList(className);
            return element;
        }
    }
}
