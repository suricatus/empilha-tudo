using System.Collections.Generic;
using NUnit.Framework;

namespace Suricatus.EmpilhaTudo.Tests
{
    public class EmpilhaContentTests
    {
        [Test]
        public void EmptyContentUsesDefaults()
        {
            var content = EmpilhaContent.Parse("");
            Assert.AreEqual(0.65f, content.StartWidth, 1e-5);
            Assert.AreEqual(0.4f, content.StartSpeed, 1e-5);
            Assert.AreEqual(100, content.PointsPerFloor);
            Assert.AreEqual(50, content.PerfectBonus);
            Assert.AreEqual(3, content.Tiers.Count);
            Assert.AreEqual(3, content.HowToPlay.Count);
        }

        [Test]
        public void ReadsSpeedWidthScoringAndSteps()
        {
            var content = EmpilhaContent.Parse(@"{
                ""larguraInicial"": 0.5,
                ""velocidade"": { ""inicial"": 0.6, ""porAndar"": 0.05, ""maxima"": 1.5 },
                ""encaixePerfeito"": 0.03,
                ""apoioMinimo"": 0.4,
                ""pontosPorAndar"": 10,
                ""bonusPerfeito"": 5,
                ""comoJogar"": [""Toque para soltar""]
            }");
            Assert.AreEqual(0.5f, content.StartWidth, 1e-5);
            Assert.AreEqual(0.6f, content.StartSpeed, 1e-5);
            Assert.AreEqual(0.05f, content.SpeedPerFloor, 1e-5);
            Assert.AreEqual(1.5f, content.MaxSpeed, 1e-5);
            Assert.AreEqual(0.03f, content.PerfectTolerance, 1e-5);
            Assert.AreEqual(0.4f, content.MinSupport, 1e-5);
            Assert.AreEqual(10, content.PointsPerFloor);
            Assert.AreEqual(5, content.PerfectBonus);
            CollectionAssert.AreEqual(new[] { "Toque para soltar" }, content.HowToPlay);
        }

        [Test]
        public void KeepsValuesInsideSafeLimits()
        {
            var content = EmpilhaContent.Parse(@"{
                ""larguraInicial"": 2,
                ""velocidade"": { ""inicial"": 0.8, ""maxima"": 0.2, ""porAndar"": -1 },
                ""encaixePerfeito"": 1,
                ""apoioMinimo"": 5,
                ""pontosPorAndar"": -5
            }");
            Assert.AreEqual(0.8f, content.StartWidth, 1e-5);
            Assert.AreEqual(0.8f, content.MaxSpeed, 1e-5, "a velocidade máxima nunca fica abaixo da inicial");
            Assert.AreEqual(0f, content.SpeedPerFloor, 1e-5);
            Assert.AreEqual(0.1f, content.PerfectTolerance, 1e-5);
            Assert.AreEqual(0.9f, content.MinSupport, 1e-5);
            Assert.AreEqual(0, content.PointsPerFloor);
        }

        [Test]
        public void SpeedGrowsPerFloorUpToTheMaximum()
        {
            var content = new EmpilhaContent { StartSpeed = 0.5f, SpeedPerFloor = 0.1f, MaxSpeed = 0.75f };
            Assert.AreEqual(0.5f, content.SpeedFor(1), 1e-5);
            Assert.AreEqual(0.7f, content.SpeedFor(3), 1e-5);
            Assert.AreEqual(0.75f, content.SpeedFor(10), 1e-5);
        }
    }

    public class TierTests
    {
        [Test]
        public void TiersAreSortedAndWithoutFlagsAllButTheLowestWin()
        {
            var content = EmpilhaContent.Parse(@"{ ""faixas"": [
                { ""nome"": ""Ouro"", ""pontos"": 900 },
                { ""nome"": ""Bronze"", ""pontos"": 0 },
                { ""nome"": ""Prata"", ""pontos"": 500 }
            ] }");
            CollectionAssert.AreEqual(new[] { "Bronze", "Prata", "Ouro" }, content.Tiers.ConvertAll(t => t.Name));
            Assert.IsFalse(content.Tiers[0].Win);
            Assert.IsTrue(content.Tiers[1].Win);
            Assert.IsTrue(content.Tiers[2].Win);
        }

        [Test]
        public void ExplicitFlagsDecideTheWins()
        {
            var content = EmpilhaContent.Parse(@"{ ""faixas"": [
                { ""nome"": ""Bronze"", ""pontos"": 0 },
                { ""nome"": ""Prata"", ""pontos"": 500 },
                { ""nome"": ""Ouro"", ""pontos"": 900, ""vitoria"": true }
            ] }");
            Assert.IsFalse(content.Tiers[1].Win, "com alguma faixa marcada, só as marcadas são vitória");
            Assert.IsTrue(content.Tiers[2].Win);
        }

        [Test]
        public void FindsTheCurrentAndTheNextTier()
        {
            var content = new EmpilhaContent();
            Assert.AreEqual("Medalha de Bronze", content.TierFor(0).Name);
            Assert.AreEqual("Medalha de Prata", content.TierFor(2000).Name);
            Assert.AreEqual("Medalha de Prata", content.NextTierAfter(1999).Name);
            Assert.AreEqual("Medalha de Ouro", content.NextTierAfter(2000).Name);
            Assert.IsNull(content.NextTierAfter(4000), "na faixa mais alta não há próxima");
        }

        [Test]
        public void ScoreBelowEveryTierHasNoTier()
        {
            var content = EmpilhaContent.Parse(@"{ ""faixas"": [ { ""nome"": ""Prata"", ""pontos"": 500 } ] }");
            Assert.IsNull(content.TierFor(100));
        }
    }

    public class StackMatchTests
    {
        // Base de 0,2 a 0,8; a peça anda 0,5 por segundo, sem acelerar.
        static EmpilhaContent Content(float minSupport = 0.2f) => new EmpilhaContent
        {
            StartWidth = 0.6f,
            StartSpeed = 0.5f,
            SpeedPerFloor = 0f,
            MaxSpeed = 0.5f,
            PerfectTolerance = 0.01f,
            MinSupport = minSupport,
        };

        /// <summary>Faz a próxima peça aparecer e a leva até <paramref name="left"/> na primeira passagem.</summary>
        static void MoveTo(StackMatch match, float left)
        {
            if (match.Moving == null) match.Tick(StackMatch.NextBlockDelay);
            var moving = match.Moving;
            bool fromLeft = moving.Floor % 2 == 1;
            float distance = fromLeft ? left - moving.Left : moving.Left - left;
            match.Tick(distance / match.Speed);
            Assert.AreEqual(left, match.Moving.Left, 1e-4, "a peça chegou onde o teste queria");
        }

        [Test]
        public void BaseIsCenteredWithTheStartWidth()
        {
            var match = new StackMatch(Content());
            Assert.AreEqual(0, match.Floors);
            Assert.AreEqual(0.2f, match.Base.Left, 1e-5);
            Assert.AreEqual(0.6f, match.Base.Width, 1e-5);
            Assert.IsNull(match.Moving);
        }

        [Test]
        public void FirstBlockStartsOnTheLeftWithTheWidthOfTheBase()
        {
            var match = new StackMatch(Content());
            Block spawned = null;
            match.Spawned += b => spawned = b;
            match.Tick(0.016f);
            Assert.IsNotNull(spawned);
            Assert.AreSame(spawned, match.Moving);
            Assert.AreEqual(1, spawned.Floor);
            Assert.AreEqual(0f, spawned.Left, 1e-5);
            Assert.AreEqual(0.6f, spawned.Width, 1e-5);
        }

        [Test]
        public void BlockBouncesBetweenTheEdges()
        {
            var match = new StackMatch(Content());
            match.Tick(0.016f);
            match.Tick(1f);   // 0,5 para a direita: bate em 0,4 e volta 0,1
            Assert.AreEqual(0.3f, match.Moving.Left, 1e-4);
            match.Tick(0.8f); // 0,4 para a esquerda: bate em 0 e volta 0,1
            Assert.AreEqual(0.1f, match.Moving.Left, 1e-4);
            match.Tick(10f);
            Assert.That(match.Moving.Left, Is.InRange(0f, 0.4f), "um quadro longo nunca tira a peça da área");
        }

        [Test]
        public void AlignedDropIsPerfectKeepsTheWidthAndEarnsTheBonus()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.205f);
            var drop = match.Drop();
            Assert.AreEqual(DropOutcome.Perfect, drop.Outcome);
            Assert.AreEqual(150, drop.Points);
            Assert.IsNull(drop.Fallen);
            Assert.AreEqual(0.2f, drop.Placed.Left, 1e-5, "o encaixe perfeito alinha a peça com a de baixo");
            Assert.AreEqual(0.6f, drop.Placed.Width, 1e-5);
            Assert.AreEqual(1, match.Floors);
            Assert.AreEqual(1, match.Perfects);
            Assert.AreEqual(1, match.PerfectStreak);
        }

        [Test]
        public void OffsetDropCutsThePartWithoutSupport()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.3f);
            var drop = match.Drop();
            Assert.AreEqual(DropOutcome.Placed, drop.Outcome);
            Assert.AreEqual(100, drop.Points);
            Assert.AreEqual(0.3f, drop.Placed.Left, 1e-4);
            Assert.AreEqual(0.5f, drop.Placed.Width, 1e-4);
            Assert.AreEqual(0.8f, drop.Fallen.Left, 1e-4, "o que passou da direita da base cai");
            Assert.AreEqual(0.1f, drop.Fallen.Width, 1e-4);
            Assert.AreSame(drop.Placed, match.Top);
        }

        [Test]
        public void NextBlockComesFromTheRightWithTheNewWidth()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.3f);
            match.Drop();
            Assert.IsNull(match.Moving, "entre um toque e a próxima peça não há peça");
            match.Tick(StackMatch.NextBlockDelay);
            Assert.AreEqual(2, match.Moving.Floor);
            Assert.AreEqual(0.5f, match.Moving.Width, 1e-4);
            Assert.AreEqual(0.5f, match.Moving.Left, 1e-4, "os andares pares saem da direita");
        }

        [Test]
        public void DropOnTheLeftCutsTheLeftSide()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.2f);
            match.Drop();
            MoveTo(match, 0.1f);
            var drop = match.Drop();
            Assert.AreEqual(DropOutcome.Placed, drop.Outcome);
            Assert.AreEqual(0.2f, drop.Placed.Left, 1e-4);
            Assert.AreEqual(0.5f, drop.Placed.Width, 1e-4);
            Assert.AreEqual(0.1f, drop.Fallen.Left, 1e-4);
            Assert.AreEqual(0.1f, drop.Fallen.Width, 1e-4);
            Assert.AreEqual(0, match.PerfectStreak, "um encaixe com corte zera a sequência de perfeitos");
        }

        [Test]
        public void LittleSupportCollapsesTheTowerAndEndsTheMatch()
        {
            var match = new StackMatch(Content(minSupport: 0.25f));
            MoveTo(match, 0.4f);   // apoio de 0,4 numa peça de 0,6: passa
            match.Drop();
            MoveTo(match, 0.2f);   // apoio de 0,2 numa peça de 0,4: metade, passa
            match.Drop();
            MoveTo(match, 0.57f);  // apoio de 0,03 numa peça de 0,2: menos de 25%
            var drop = match.Drop();
            Assert.AreEqual(DropOutcome.Collapsed, drop.Outcome);
            Assert.AreEqual(0, drop.Points);
            Assert.IsNull(drop.Placed);
            Assert.AreSame(drop.Dropped, drop.Fallen, "no desabamento a peça cai inteira");
            Assert.IsTrue(match.Collapsed);
            Assert.IsTrue(match.Stopped);
            Assert.AreEqual(2, match.Floors);
            Assert.AreEqual(3, match.Drops);
            Assert.AreEqual(DropOutcome.Ignored, match.Drop().Outcome);
            match.Tick(1f);
            Assert.IsNull(match.Moving, "depois do desabamento não vem outra peça");
        }

        [Test]
        public void NarrowerThanTheMinimumWidthCollapsesEvenWithoutMinSupport()
        {
            var match = new StackMatch(Content(minSupport: 0f));
            MoveTo(match, 0.4f);
            match.Drop();
            MoveTo(match, StackMatch.MinWidth / 2); // apoio de meia largura mínima
            Assert.AreEqual(DropOutcome.Collapsed, match.Drop().Outcome);
        }

        [Test]
        public void MissingTheTowerCompletelyCollapses()
        {
            var match = new StackMatch(Content(minSupport: 0f));
            MoveTo(match, 0.3f);
            match.Drop();
            MoveTo(match, 0f);
            var drop = match.Drop();
            Assert.AreEqual(DropOutcome.Placed, drop.Outcome, "apoio de 0,2 numa peça de 0,5");
            match.Tick(StackMatch.NextBlockDelay);
            match.Tick(1.2f);
            Assert.AreEqual(DropOutcome.Collapsed, match.Drop().Outcome, "longe da torre não há apoio");
        }

        [Test]
        public void TapBetweenBlocksIsIgnored()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.3f);
            match.Drop();
            Assert.AreEqual(DropOutcome.Ignored, match.Drop().Outcome);
            Assert.AreEqual(1, match.Drops);
        }

        [Test]
        public void StopFreezesTheMovingBlock()
        {
            var match = new StackMatch(Content());
            MoveTo(match, 0.1f);
            match.Stop();
            match.Tick(1f);
            Assert.AreEqual(0.1f, match.Moving.Left, 1e-4);
            Assert.AreEqual(DropOutcome.Ignored, match.Drop().Outcome);
        }

        [Test]
        public void SpeedGrowsWithTheTower()
        {
            var content = Content();
            content.SpeedPerFloor = 0.1f;
            content.MaxSpeed = 2f;
            var match = new StackMatch(content);
            match.Tick(0.016f);
            Assert.AreEqual(0.5f, match.Speed, 1e-5);
            MoveTo(match, 0.2f);
            match.Drop();
            match.Tick(StackMatch.NextBlockDelay);
            Assert.AreEqual(0.6f, match.Speed, 1e-5);
        }

        [Test]
        public void VariantsCycleFloorByFloor()
        {
            var match = new StackMatch(Content(), variants: 3);
            var variants = new List<int>();
            match.Spawned += b => variants.Add(b.Variant);
            for (int i = 0; i < 4; i++)
            {
                MoveTo(match, 0.2f);
                match.Drop();
            }
            match.Tick(StackMatch.NextBlockDelay);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 0, 1 }, variants);
        }
    }
}
