using System.Collections.Generic;
using Suricatus.FastDrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>
    /// A mecânica do Empilha Tudo. Tudo o mais (telas, placar, tempo, pacotes) vem da Base do Fast-Drive.
    /// A velocidade, o tamanho das peças, a pontuação e as faixas vêm do conteudo/empilha.json do cliente;
    /// as artes, da pasta jogo/.
    /// </summary>
    public sealed class EmpilhaTudoGameplay : GameplayProvider
    {
        const string DefaultTitle = "Empilha Tudo";
        const string DefaultSubtitle = "Empilhe as peças e construa a torre mais alta!";
        const string DefaultResult = "Altura: {andares} · Perfeitos: {perfeitos}";

        GameContext context;
        EmpilhaContent content = new EmpilhaContent();
        GameArts arts;
        MatchContext match;
        StackMatch state;
        TowerView tower;
        MatchPanelView panel;
        int lastFloors;
        int lastPerfects;

        public override string Title => context != null ? context.Texts.Get("empilha.titulo", DefaultTitle) : DefaultTitle;

        public override string Subtitle => context?.Texts.Get("empilha.subtitulo", DefaultSubtitle);

        public override IReadOnlyList<string> HowToPlay => content.HowToPlay;

        public override void Configure(GameContext gameContext)
        {
            context = gameContext;
            var asset = context.Bundle.FindContent(EmpilhaContent.FileName);
            if (asset == null)
            {
                Debug.LogWarning($"[Empilha Tudo] O cliente \"{context.Bundle.clientId}\" não tem conteudo/{EmpilhaContent.FileName}.json: valem os padrões do jogo.");
            }
            else
            {
                try
                {
                    content = EmpilhaContent.Parse(asset.text);
                }
                catch (System.FormatException e)
                {
                    Debug.LogError($"[Empilha Tudo] conteudo/{EmpilhaContent.FileName}.json: {e.Message}");
                }
            }

            arts = new GameArts(context.Bundle);
            if (arts.Pieces.Count == 0)
                Debug.LogWarning($"[Empilha Tudo] O cliente \"{context.Bundle.clientId}\" não tem jogo/peca.png: as peças vão ser desenhadas com as cores do tema.");
            if (arts.Base == null)
                Debug.LogWarning($"[Empilha Tudo] O cliente \"{context.Bundle.clientId}\" não tem jogo/base.png: a base vai ser desenhada com as cores do tema.");

            var styles = Resources.Load<StyleSheet>("EmpilhaTudo");
            if (styles != null) context.Root.styleSheets.Add(styles);
        }

        public override void BeginMatch(MatchContext matchContext, VisualElement gameArea, VisualElement supportArea)
        {
            match = matchContext;
            state = new StackMatch(content, arts.Pieces.Count);
            lastFloors = 0;
            lastPerfects = 0;

            tower = new TowerView(context.Ui, context.Texts, arts, state, Drop);
            gameArea.Add(tower);
            panel = new MatchPanelView(context.Ui, context.Texts, arts, content, Drop);
            supportArea.Add(panel);

            state.Spawned += tower.ShowMoving;
            panel.SetProgress(0, 0, 0);
        }

        public override void TickMatch(float deltaTime)
        {
            if (state == null || match.Finished) return;
            state.Tick(deltaTime);
            tower.Tick();
        }

        public override void TimeUp()
        {
            if (state == null) return;
            state.Stop();
            tower.Freeze();
            Finish();
        }

        public override void EndMatch()
        {
            if (state != null && tower != null) state.Spawned -= tower.ShowMoving;
            tower?.RemoveFromHierarchy();
            panel?.RemoveFromHierarchy();
            tower = null;
            panel = null;
            state = null;
            match = null;
        }

        public override string ResultDetail(GameResult result)
        {
            var line = context.Texts.Get("empilha.resultado", DefaultResult)
                .Replace("{andares}", lastFloors.ToString())
                .Replace("{perfeitos}", lastPerfects.ToString());
            var tier = content.TierFor(result.Score);
            return tier == null ? line : "<b>" + tier.Name + "</b>\n" + line;
        }

        /// <summary>O toque, na área do jogo ou na de apoio.</summary>
        void Drop()
        {
            if (state == null || match.Finished) return;
            var drop = state.Drop();
            switch (drop.Outcome)
            {
                case DropOutcome.Placed:
                case DropOutcome.Perfect:
                    match.Hit(drop.Points);
                    tower.Place(drop, state.PerfectStreak);
                    break;
                case DropOutcome.Collapsed:
                    match.Miss();
                    tower.Collapse(drop);
                    Finish();
                    break;
                default:
                    return;
            }
            panel.SetProgress(state.Floors, state.Perfects, match.Score);
        }

        /// <summary>Fim da partida, pelo desabamento ou pelo tempo: vale a faixa da pontuação.</summary>
        void Finish()
        {
            lastFloors = state.Floors;
            lastPerfects = state.Perfects;
            var tier = content.TierFor(match.Score);
            match.Finish(tier != null && tier.Win, state.Floors, state.Drops);
        }
    }
}
