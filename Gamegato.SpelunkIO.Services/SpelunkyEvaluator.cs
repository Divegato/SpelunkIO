using System;
using System.Threading;
using Gamegato.SpelunkIO.Connector;
using Gamegato.SpelunkIO.Services.Outputs;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace Gamegato.SpelunkIO.Services
{
    public class SpelunkyEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        ulong _evalCount;
        SpelunkyHooks hooks;
        SpelunkyOutput output;
        ButtonActions[] outputActions;
        bool _stopConditionSatisfied;
        Random r;

        public SpelunkyEvaluator()
        {
            hooks = new SpelunkyHooks(new RawProcess("Spelunky"));
            output = new SpelunkyOutput();
            outputActions = new[]
            {
                ButtonActions.Left,
                ButtonActions.Right,
                ButtonActions.Jump,
                ButtonActions.WhipAction,
                ButtonActions.Up,
                ButtonActions.Duck,
                ButtonActions.Bomb,
                ButtonActions.Rope,
                ButtonActions.Run,
                ButtonActions.PurchaseDoor,
            };
            r = new Random(DateTime.Now.Millisecond);
        }

        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        public FitnessInfo Evaluate(IBlackBox box)
        {
            while (hooks.CurrentState != SpelunkyState.Active)
            {
                Console.WriteLine("State: {0}", hooks.CurrentState);
                Thread.Sleep(1000);
            }

            var start = DateTime.Now;

            _evalCount++;
            Console.WriteLine("Attempt: {0}", _evalCount);
            while (hooks.StageSeconds < 20 && hooks.CharacterHearts > 0)
            {
                // Activate inputs
                box.InputSignalArray[0] = hooks.CharacterHearts / 100;
                // Todo: Replace with Spelunky's visible tiles
                for (int i = 1; i < box.InputCount; i++)
                {
                    box.InputSignalArray[i] = (r.NextDouble() * 2) - 1;
                }

                // Active box
                box.Activate();

                // Activate outputs
                for (var i = 0; i < box.OutputCount; i++)
                {
                    var outputSignal = box.OutputSignalArray[i];

                    if (outputSignal > 0.5)
                    {
                        output.PressKey(outputActions[i]);
                    }

                    if (outputSignal < 0.5)
                    {
                        output.LiftKey(outputActions[i]);
                    }
                }
            }

            var fitness = CalculateFitness();

            Reset();

            _stopConditionSatisfied = true;

            return fitness;
        }

        public FitnessInfo CalculateFitness()
        {
            Console.WriteLine("Level: {0} Health: {1}", hooks.CurrentLevel, hooks.CharacterHearts);
            return new FitnessInfo((double)hooks.CurrentLevel, (double)hooks.CurrentLevel);
        }

        public void Reset()
        {
            foreach (var action in outputActions)
            {
                output.LiftKey(action);
            }

            if (hooks.CharacterHearts > 0)
            {
                output.PressEscape();
                Thread.Sleep(100);
                output.PressDown();
                output.PressKey(ButtonActions.Jump);
                Thread.Sleep(100);
                output.LiftKey(ButtonActions.Jump);
                Thread.Sleep(100);
                output.PressKey(ButtonActions.Jump);
                Thread.Sleep(100);
                output.LiftKey(ButtonActions.Jump);
                Thread.Sleep(4000);
                output.PressKey(ButtonActions.WhipAction);
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(4000);
                output.PressKey(ButtonActions.WhipAction);
                Thread.Sleep(1000);
            }
        }
    }
}