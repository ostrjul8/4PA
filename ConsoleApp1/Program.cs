class Item
{
    public int Value { get; set; }
    public int Weight { get; set; }
}

class Individual
{
    public bool[] Genes { get; set; }
    public int Fitness { get; set; }

    public Individual(int numberOfItems)
    {
        Genes = new bool[numberOfItems];
    }
}

class Program
{
    static Random random = new Random();
    const int Capacity = 250;
    const int NumberOfItems = 100;
    const int PopulationSize = 100;

    static List<Item> GenerateItems()
    {
        var items = new List<Item>();
        for (int i = 0; i < NumberOfItems; i++)
        {
            items.Add(new Item
            {
                Value = random.Next(2, 31),
                Weight = random.Next(1, 26)
            });
        }
        return items;
    }

    static List<Individual> InitializePopulation(int numberOfItems, List<Item> items)
    {
        var population = new List<Individual>();

        for (int i = 0; i < PopulationSize; i++)
        {
            var individual = new Individual(numberOfItems);
            int totalWeight = 0;

            var randomItems = items.OrderBy(x => random.Next()).ToList();

            foreach (var item in randomItems)
            {
                if (totalWeight + item.Weight <= Capacity)
                {
                    individual.Genes[items.IndexOf(item)] = true;
                    totalWeight += item.Weight;
                }
            }

            population.Add(individual);
        }
        return population;
    }

    static void EvaluateFitness(Individual individual, List<Item> items)
    {
        int totalValue = 0;
        int totalWeight = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (individual.Genes[i])
            {
                totalValue += items[i].Value;
                totalWeight += items[i].Weight;
            }
        }

        if (totalWeight > Capacity)
        {
            individual.Fitness = 0;
        }
        else
        {
            individual.Fitness = totalValue;
        }
    }

    static void Crossover(Individual parent1, Individual parent2, out Individual offspring1, out Individual offspring2)
    {
        offspring1 = new Individual(NumberOfItems);
        offspring2 = new Individual(NumberOfItems);

        int point1 = random.Next(NumberOfItems);
        int point2 = random.Next(point1, NumberOfItems);
        int point3 = random.Next(point2, NumberOfItems);

        for (int i = 0; i < NumberOfItems; i++)
        {
            if (i < point1 || (i >= point2 && i < point3))
            {
                offspring1.Genes[i] = parent1.Genes[i];
                offspring2.Genes[i] = parent2.Genes[i];
            }
            else
            {
                offspring1.Genes[i] = parent2.Genes[i];
                offspring2.Genes[i] = parent1.Genes[i];
            }
        }
    }

    static void Mutate(Individual individual)
    {
        if (random.NextDouble() < 0.05)
        {
            int geneToMutate = random.Next(NumberOfItems);
            individual.Genes[geneToMutate] = !individual.Genes[geneToMutate];
        }
    }

    static void LocalImprovement(Individual individual, List<Item> items)
    {
        int totalWeight = individual.Genes.Select((gene, index) => gene ? items[index].Weight : 0).Sum();
        int totalValue = individual.Genes.Select((gene, index) => gene ? items[index].Value : 0).Sum();

        for (int i = 0; i < items.Count; i++)
        {
            if (!individual.Genes[i])
            {
                if (totalWeight + items[i].Weight <= Capacity)
                {
                    individual.Genes[i] = true;
                    totalWeight += items[i].Weight;
                    totalValue += items[i].Value;
                }
            }
            else
            {
                double bestValuePerWeight = (double)items[i].Value / items[i].Weight;
                int bestItemIndex = i;
                double bestItemValuePerWeight = bestValuePerWeight;

                int weightWithoutCurrentItem = totalWeight - items[i].Weight;

                for (int j = 0; j < items.Count; j++)
                {
                    if (!individual.Genes[j] && weightWithoutCurrentItem + items[j].Weight <= Capacity)
                    {
                        double valuePerWeight = (double)items[j].Value / items[j].Weight;

                        if (valuePerWeight > bestItemValuePerWeight)
                        {
                            bestItemValuePerWeight = valuePerWeight;
                            bestItemIndex = j;
                        }
                    }
                }

                if (bestItemIndex != i)
                {
                    individual.Genes[i] = false;
                    totalWeight -= items[i].Weight;
                    totalValue -= items[i].Value;

                    individual.Genes[bestItemIndex] = true;
                    totalWeight += items[bestItemIndex].Weight;
                    totalValue += items[bestItemIndex].Value;
                }
            }

            individual.Fitness = totalValue;
        }
    }

    static void Main()
    {
        List<Item> items = GenerateItems();
        List<Individual> population = InitializePopulation(NumberOfItems, items);

        for (int generation = 1; generation <= 1000; generation++)
        {
            foreach (var individual in population)
            {
                EvaluateFitness(individual, items);
            }

            population = population.OrderByDescending(ind => ind.Fitness).ToList();
            List<Individual> newPopulation = new List<Individual>();

            for (int i = 0; i < PopulationSize / 2; i++)
            {
                Individual parent1 = population[random.Next(PopulationSize)];
                Individual parent2 = population[random.Next(PopulationSize)];

                if (random.NextDouble() < 0.25)
                {
                    Individual offspring1, offspring2;
                    Crossover(parent1, parent2, out offspring1, out offspring2);

                    Mutate(offspring1);
                    Mutate(offspring2);
                    LocalImprovement(offspring1, items);
                    LocalImprovement(offspring2, items);

                    newPopulation.Add(offspring1);
                    newPopulation.Add(offspring2);
                }
                else
                {
                    Mutate(parent1);
                    Mutate(parent2);
                    LocalImprovement(parent1, items);
                    LocalImprovement(parent2, items);

                    newPopulation.Add(parent1);
                    newPopulation.Add(parent2);
                }
            }
            population = newPopulation;

            if (generation % 10 == 0)
            {
                Individual bestIndividual = population.OrderByDescending(ind => ind.Fitness).First();
                Console.WriteLine($"Generation {generation} - Best Fitness: {bestIndividual.Fitness}");
            }
        }

        Individual finalBestIndividual = population.OrderByDescending(ind => ind.Fitness).First();
        Console.WriteLine($"Найкраща цінність після 1000 поколінь: {finalBestIndividual.Fitness}");
    }
}

