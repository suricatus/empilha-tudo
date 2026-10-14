using System;
using System.Collections.Generic;

namespace Suricatus.EmpilhaTudo
{
    /// <summary>
    /// Uma peça: a base (andar 0), uma peça da torre ou a que está se movendo.
    /// Posição e largura em fração da largura da área do jogo (0 a 1).
    /// </summary>
    public sealed class Block
    {
        public readonly int Floor;
        public readonly float Width;
        /// <summary>Qual das artes do cliente usar (peca-1, peca-2…). A base não tem variação.</summary>
        public readonly int Variant;

        public Block(int floor, float left, float width, int variant)
        {
            Floor = floor;
            Left = left;
            Width = width;
            Variant = variant;
        }

        public float Left { get; internal set; }
        public float Right => Left + Width;
        public float Center => Left + Width / 2;
    }

    public enum DropOutcome
    {
        /// <summary>Não havia peça para soltar (entre uma peça e a próxima, ou partida parada).</summary>
        Ignored,
        /// <summary>A peça encaixou e a parte sem apoio caiu.</summary>
        Placed,
        /// <summary>A peça encaixou inteira, alinhada com a de baixo: vale o bônus.</summary>
        Perfect,
        /// <summary>A peça ficou com pouco ou nenhum apoio: a torre desaba e a partida termina.</summary>
        Collapsed
    }

    public readonly struct DropResult
    {
        public readonly DropOutcome Outcome;
        public readonly int Points;
        /// <summary>A peça como estava no momento do toque.</summary>
        public readonly Block Dropped;
        /// <summary>O que ficou na torre. Null no desabamento.</summary>
        public readonly Block Placed;
        /// <summary>A parte sem apoio, que cai. Null no encaixe perfeito; no desabamento, a peça inteira.</summary>
        public readonly Block Fallen;

        public DropResult(DropOutcome outcome, int points, Block dropped, Block placed, Block fallen)
        {
            Outcome = outcome;
            Points = points;
            Dropped = dropped;
            Placed = placed;
            Fallen = fallen;
        }
    }

    /// <summary>
    /// Uma partida do Empilha Tudo, sem Unity. A peça vai e volta na largura da área do jogo; ao toque, o que fica
    /// sobre a peça de baixo vira um andar e o resto cai. Quem desenha escuta <see cref="Spawned"/>.
    /// </summary>
    public sealed class StackMatch
    {
        /// <summary>Espera entre um toque e a próxima peça, para a parte cortada começar a cair.</summary>
        public const float NextBlockDelay = 0.15f;
        /// <summary>A peça mais estreita que fica de pé, em fração da largura da área do jogo.</summary>
        public const float MinWidth = 0.04f;

        readonly EmpilhaContent content;
        readonly int variants;
        readonly List<Block> tower = new List<Block>();
        float spawnIn;
        int direction;

        public StackMatch(EmpilhaContent content, int variants = 1)
        {
            this.content = content;
            this.variants = Math.Max(1, variants);
            tower.Add(new Block(0, (1 - content.StartWidth) / 2, content.StartWidth, 0));
        }

        /// <summary>Uma peça nova começou a se mover (logo no início e depois de cada encaixe).</summary>
        public event Action<Block> Spawned;

        /// <summary>A base e os andares, de baixo para cima.</summary>
        public IReadOnlyList<Block> Tower => tower;
        public Block Base => tower[0];
        public Block Top => tower[tower.Count - 1];
        /// <summary>A peça que está se movendo. Null entre um toque e a próxima peça.</summary>
        public Block Moving { get; private set; }
        public float Speed { get; private set; }
        /// <summary>Andares encaixados (a altura da torre).</summary>
        public int Floors => tower.Count - 1;
        public int Perfects { get; private set; }
        /// <summary>Encaixes perfeitos seguidos, até agora.</summary>
        public int PerfectStreak { get; private set; }
        /// <summary>Peças soltas, incluindo a que derrubou a torre.</summary>
        public int Drops { get; private set; }
        public bool Collapsed { get; private set; }
        public bool Stopped { get; private set; }

        public void Tick(float deltaTime)
        {
            if (Stopped || deltaTime <= 0) return;
            if (Moving == null)
            {
                spawnIn -= deltaTime;
                if (spawnIn <= 0) Spawn();
                return;
            }
            Move(Moving, deltaTime);
        }

        /// <summary>O toque: solta a peça que está se movendo.</summary>
        public DropResult Drop()
        {
            if (Stopped || Moving == null) return new DropResult(DropOutcome.Ignored, 0, null, null, null);

            var moving = Moving;
            var top = Top;
            Moving = null;
            Drops++;

            if (Math.Abs(moving.Left - top.Left) <= content.PerfectTolerance)
            {
                var whole = new Block(moving.Floor, top.Left, Math.Min(moving.Width, top.Width), moving.Variant);
                tower.Add(whole);
                Perfects++;
                PerfectStreak++;
                spawnIn = NextBlockDelay;
                return new DropResult(DropOutcome.Perfect, content.PointsPerFloor + content.PerfectBonus, moving, whole, null);
            }

            PerfectStreak = 0;
            float left = Math.Max(moving.Left, top.Left);
            float right = Math.Min(moving.Right, top.Right);
            float overlap = right - left;
            if (overlap < Math.Max(content.MinSupport * moving.Width, MinWidth))
            {
                Collapsed = true;
                Stopped = true;
                return new DropResult(DropOutcome.Collapsed, 0, moving, null, moving);
            }

            var placed = new Block(moving.Floor, left, overlap, moving.Variant);
            var fallen = moving.Left > top.Left
                ? new Block(moving.Floor, right, moving.Right - right, moving.Variant)
                : new Block(moving.Floor, moving.Left, left - moving.Left, moving.Variant);
            tower.Add(placed);
            spawnIn = NextBlockDelay;
            return new DropResult(DropOutcome.Placed, content.PointsPerFloor, moving, placed, fallen);
        }

        /// <summary>Fim do tempo: a peça para onde está.</summary>
        public void Stop() => Stopped = true;

        void Spawn()
        {
            var top = Top;
            int floor = top.Floor + 1;
            // Os andares alternam o lado de onde a peça sai: ímpares da esquerda, pares da direita.
            bool fromLeft = floor % 2 == 1;
            direction = fromLeft ? 1 : -1;
            Speed = content.SpeedFor(floor);
            Moving = new Block(floor, fromLeft ? 0 : MaxLeft(top.Width), top.Width, (floor - 1) % variants);
            Spawned?.Invoke(Moving);
        }

        /// <summary>A peça vai de uma borda à outra da área do jogo e volta.</summary>
        void Move(Block block, float deltaTime)
        {
            float max = MaxLeft(block.Width);
            if (max <= 0)
            {
                block.Left = 0;
                return;
            }
            float left = block.Left + direction * Speed * deltaTime;
            // Um quadro muito longo pode bater nas duas bordas: o laço rebate quantas vezes for preciso.
            while (left < 0 || left > max)
            {
                if (left > max)
                {
                    left = 2 * max - left;
                    direction = -1;
                }
                else
                {
                    left = -left;
                    direction = 1;
                }
            }
            block.Left = left;
        }

        static float MaxLeft(float width) => Math.Max(0, 1 - width);
    }
}
