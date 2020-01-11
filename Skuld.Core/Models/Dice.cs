using System;
using System.Collections.Generic;

namespace Skuld.Core.Models
{
    public class Dice
    {
        private List<Die> dies;

        public Dice()
        {
            dies = new List<Die>();
        }

        public Dice(int amount) : this()
        {
            var random = new Random();
            for (int x = 0; x < amount; x++)
            {
                dies.Add(new Die(random));
            }
        }

        public Dice(int amount, Random random) : this()
        {
            for (int x = 0; x < amount; x++)
            {
                dies.Add(new Die(random));
            }
        }

        public void SetDice(int amount, Random random = null)
        {
            dies.Clear();

            for (int x = 0; x < amount; x++)
            {
                dies.Add(new Die(random ?? new Random()));
            }
        }

        public ulong GetSumOfFaces()
        {
            ulong amount = 0;

            foreach (var die in dies)
            {
                amount += die.Face;
            }
            return amount;
        }

        public ushort[] GetFaces()
        {
            List<ushort> Face = new List<ushort>();
            foreach (var die in dies)
            {
                Face.Add(die.Face);
            }
            return Face.ToArray();
        }

        public Die[] GetDies()
            => dies.ToArray();

        public Die[] Roll()
        {
            dies.ForEach(x =>
            {
                x = x.Roll();
            });

            return dies.ToArray();
        }
    }
}