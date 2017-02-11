using System.Collections.Generic;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;

namespace Gamegato.SpelunkIO.Services
{
    public class SpelunkyAlgorithm
    {
        private NeatEvolutionAlgorithmParameters eaParams;
        private NeatGenomeParameters neatGenomeParams;

        public SpelunkyAlgorithm(NeatEvolutionAlgorithmParameters eaParams, NeatGenomeParameters neatGenomeParams)
        {
            this.eaParams = eaParams;
            this.neatGenomeParams = neatGenomeParams;
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 };
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, parallelOptions);

            IComplexityRegulationStrategy complexityRegulationStrategy = new NullComplexityRegulationStrategy();
            //ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParams, speciationStrategy, complexityRegulationStrategy);

            SpelunkyEvaluator evaluator = new SpelunkyEvaluator();

            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();
            IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, parallelOptions);
            IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(innerEvaluator, SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_PeriodicReevaluation(5));
            ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);

            return ea;
        }

        public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
        {
            return new NeatGenomeDecoder(NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1));
        }
    }
}
