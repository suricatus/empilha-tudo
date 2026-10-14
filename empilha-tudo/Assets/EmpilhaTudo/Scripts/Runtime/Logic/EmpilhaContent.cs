using System;
using System.Collections.Generic;
using Suricatus.FastDrive;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>Uma faixa de resultado (ex.: Medalha de Prata a partir de 2000 pontos).</summary>
    public sealed class Tier
    {
        public readonly string Name;
        public readonly int MinScore;
        /// <summary>Se a partida que chega nesta faixa conta como vitória (título, confete e som de vitória).</summary>
        public readonly bool Win;

        public Tier(string name, int minScore, bool win)
        {
            Name = name;
            MinScore = minScore;
            Win = win;
        }
    }

    /// <summary>Conteúdo do conteudo/empilha.json do cliente. Tudo é opcional; o que faltar fica com o padrão.</summary>
    public sealed class EmpilhaContent
    {
        public const string FileName = "empilha";

        public static readonly IReadOnlyList<string> DefaultHowToPlay = new[]
        {
            "Toque na tela para soltar a peça",
            "O que ficar para fora cai",
            "Encaixe perfeito vale pontos extras",
        };

        /// <summary>Largura da primeira peça (e da base), em fração da largura da área do jogo.</summary>
        public float StartWidth = 0.65f;
        /// <summary>Velocidade da peça no primeiro andar, em larguras da área do jogo por segundo.</summary>
        public float StartSpeed = 0.4f;
        /// <summary>Quanto a velocidade aumenta a cada andar.</summary>
        public float SpeedPerFloor = 0.015f;
        public float MaxSpeed = 1.0f;
        /// <summary>
        /// Distância até o alinhamento, em fração da largura da área do jogo, que ainda conta como encaixe perfeito:
        /// a peça encaixa inteira, sem cortar nada, e ganha o bônus.
        /// </summary>
        public float PerfectTolerance = 0.02f;
        /// <summary>Fração da peça que precisa ficar apoiada na de baixo. Menos que isso, a torre desaba.</summary>
        public float MinSupport = 0.2f;

        public int PointsPerFloor = 100;
        public int PerfectBonus = 50;

        public readonly List<Tier> Tiers = new List<Tier>(DefaultTiers());
        public readonly List<string> HowToPlay = new List<string>(DefaultHowToPlay);

        /// <summary>
        /// Calibradas para a partida de 40 s (simulação de jogadores): quem joga na média chega à Prata (cerca de
        /// 17 andares); quem é preciso, ao Ouro (cerca de 30).
        /// </summary>
        static IEnumerable<Tier> DefaultTiers() => new[]
        {
            new Tier("Medalha de Bronze", 0, false),
            new Tier("Medalha de Prata", 2000, true),
            new Tier("Medalha de Ouro", 4000, true),
        };

        /// <summary>Velocidade da peça que vai para o andar <paramref name="floor"/> (o primeiro andar é o 1).</summary>
        public float SpeedFor(int floor) => Math.Min(MaxSpeed, StartSpeed + SpeedPerFloor * Math.Max(0, floor - 1));

        /// <summary>A faixa mais alta que a pontuação alcança, ou null quando não alcança nenhuma.</summary>
        public Tier TierFor(int score)
        {
            Tier best = null;
            foreach (var tier in Tiers)
                if (score >= tier.MinScore) best = tier;
            return best;
        }

        /// <summary>A primeira faixa acima da pontuação, ou null quando ela já está na mais alta.</summary>
        public Tier NextTierAfter(int score)
        {
            foreach (var tier in Tiers)
                if (tier.MinScore > score) return tier;
            return null;
        }

        public static EmpilhaContent Parse(string json)
        {
            var content = new EmpilhaContent();
            if (string.IsNullOrWhiteSpace(json)) return content;

            var root = Json.ParseObject(json);
            content.StartWidth = Clamp((float)root.GetNumber("larguraInicial", content.StartWidth), 0.3f, 0.8f);

            var speed = root.GetObject("velocidade");
            if (speed != null)
            {
                content.StartSpeed = Clamp((float)speed.GetNumber("inicial", content.StartSpeed), 0.1f, 3f);
                content.SpeedPerFloor = Clamp((float)speed.GetNumber("porAndar", content.SpeedPerFloor), 0f, 0.5f);
                content.MaxSpeed = Clamp((float)speed.GetNumber("maxima", content.MaxSpeed), content.StartSpeed, 3f);
            }
            content.MaxSpeed = Math.Max(content.MaxSpeed, content.StartSpeed);

            content.PerfectTolerance = Clamp((float)root.GetNumber("encaixePerfeito", content.PerfectTolerance), 0f, 0.1f);
            content.MinSupport = Clamp((float)root.GetNumber("apoioMinimo", content.MinSupport), 0f, 0.9f);
            content.PointsPerFloor = Math.Max(0, root.GetInt("pontosPorAndar", content.PointsPerFloor));
            content.PerfectBonus = Math.Max(0, root.GetInt("bonusPerfeito", content.PerfectBonus));

            var tiers = ReadTiers(root.GetList("faixas"));
            if (tiers.Count > 0)
            {
                content.Tiers.Clear();
                content.Tiers.AddRange(tiers);
            }

            var steps = root.GetStringList("comoJogar");
            if (steps.Count > 0)
            {
                content.HowToPlay.Clear();
                content.HowToPlay.AddRange(steps);
            }
            return content;
        }

        /// <summary>
        /// Faixas em ordem de pontos. Quando nenhuma diz "vitoria", todas menos a mais baixa contam como vitória.
        /// </summary>
        static List<Tier> ReadTiers(List<object> list)
        {
            var read = new List<(string name, int points, bool? win)>();
            if (list == null) return new List<Tier>();
            foreach (var item in list)
            {
                if (!(item is Dictionary<string, object> obj)) continue;
                var name = obj.GetString("nome");
                if (string.IsNullOrWhiteSpace(name)) continue;
                bool? win = obj.ContainsKey("vitoria") ? obj.GetBool("vitoria") : (bool?)null;
                read.Add((name.Trim(), Math.Max(0, obj.GetInt("pontos", 0)), win));
            }
            read.Sort((a, b) => a.points.CompareTo(b.points));

            bool anyFlag = read.Exists(t => t.win.HasValue);
            var tiers = new List<Tier>();
            for (int i = 0; i < read.Count; i++)
                tiers.Add(new Tier(read[i].name, read[i].points, anyFlag ? read[i].win == true : i > 0));
            return tiers;
        }

        static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}
