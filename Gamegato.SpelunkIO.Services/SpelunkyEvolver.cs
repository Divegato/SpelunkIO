using System;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

namespace Gamegato.SpelunkIO.Services
{
    public static class SpelunkyEvolver
    {
        // population size = 2000
        // complexity threshold =10
        public static void Run()
        {
            var eaParams = new NeatEvolutionAlgorithmParameters
            {
                SpecieCount = 10,
                SelectionProportion = 0.5,
                ElitismProportion = 0.5,
                OffspringAsexualProportion = 0.95,
                OffspringSexualProportion = 0.05,
                InterspeciesMatingProportion = 0.00
            };

            var genomeParams = new NeatGenomeParameters
            {
                AddConnectionMutationProbability = 0.1,
                AddNodeMutationProbability = 0.01,
                ConnectionWeightMutationProbability = 0.89,
                InitialInterconnectionsProportion = 0.05
            };

            var genomeFactory = new NeatGenomeFactory(10, 10, genomeParams);
            var genomeList = genomeFactory.CreateGenomeList(2000, 0u);

            var algorithm = new SpelunkyAlgorithm(eaParams, genomeParams);

            var evolution = algorithm.CreateEvolutionAlgorithm(genomeFactory, genomeList);
            evolution.UpdateEvent += new EventHandler((object sender, EventArgs e) =>
            {
                Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N6}", evolution.CurrentGeneration, evolution.Statistics._maxFitness));
            });

            evolution.StartContinue();

            Console.ReadLine();
        }
    }
}
