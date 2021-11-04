using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

// Демонстрация оптимизации колонии муравьев (ACO), решающей задачу коммивояжера (TSP).
// Есть много вариантов ACO; это всего лишь один подход.
// Задача, которую нужно решить, имеет программно определенное количество городов. Мы предполагаем, что каждый
// город связан со всеми остальными городами. Расстояние между городами искусственно
// устанавливаем так, чтобы расстояние между любыми двумя городами было случайным значением от 1 до 8
// Города переносятся, поэтому, если есть 20 городов, то D (0,19) = D (19,0).
// Свободными параметрами являются alpha, beta, rho и Q. Жестко заданные константы ограничивают min и max
// значения феромонов.

namespace AntColony
{
    internal class AntColonyProgram
    {

        private static Random random = new Random(0);
        // влияние феромона на направление
        private static int alpha = 3;
        // влияние расстояния между соседними узлами
        private static int beta = 2;

        // фактор уменьшения феромонов
        private static double rho = 0.7;
        // фактор увеличения феромона
        private static double Q = 2;

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("\nНачало\n");

                int numCities = 200;
                int numAnts = 10;
                int numNORMants = 35;
                int maxTime = 1000;

                Console.WriteLine("Количество городов = " + numCities);

                Console.WriteLine("\nКоличество муравьёв элитных = " + numAnts);
                Console.WriteLine("\nКоличество муравьёв обычных = " + numNORMants);
                Console.WriteLine("\nКоличество всех муравьёв = " + (numNORMants+numAnts));
                Console.WriteLine("Максимум итераций = " + maxTime);

                Console.WriteLine("\nAlpha = " + alpha);
                Console.WriteLine("Beta = " + beta);
                Console.WriteLine("Rho (коэффициент испарения феромона) = " + rho.ToString("F2"));
                Console.WriteLine("Q (фактор депозита феромонов) = " + Q.ToString("F2"));

                Console.WriteLine("\nИнициализация расстояний графа");
                int[][] dists = MakeGraphDistances(numCities);

                //int razom = numAnts + numNORMants;
                //First 10
                Console.WriteLine("\nЗапуск муравьев на случайные тропы\n");
                int[][] ants = InitAnts(numAnts, numCities);
                //Second 35
                Console.WriteLine("\n=-=-=-=-=-=-=-=-=-=-=-=\n");
                int[][] ants2 = InitAnts(numNORMants, numCities);
                // инициализируем муравьев случайными следами
                ShowAnts(ants, dists);
                Console.WriteLine("\n=-=-=-=-=-=-=-=-=-=-=-=\n");
                ShowAnts(ants2, dists);



                int[] bestTrail = AntColonyProgram.BestTrail(ants, dists);
                // определяем лучший начальный следil
                double bestLength = Length(bestTrail, dists);
                // длина лучшего следа
                //  Console.Write("\nBest initial trail length: " + bestLength.ToString("F1") + "\n");
                //------Display(bestTrail);
                int[] bestTrail2 = AntColonyProgram.BestTrail(ants2, dists);
                // определяем лучший начальный след
                double bestLength2 = Length(bestTrail2, dists);
                if(bestLength > bestLength2)
                {
                    Console.Write("\nЛучшая начальная длина следа: " + bestLength2.ToString("F1") + "\n");
                }
                else
                {
                    Console.Write("\nЛучшая начальная длина следа: " + bestLength.ToString("F1") + "\n");
                }
               



                Console.WriteLine("\nИнициализация феромонов на тропах");
                double[][] pheromones = InitPheromones(numCities);

                int time = 0;
                Console.WriteLine("\nВход в UpdateAnts - цикл UpdatePheromones\n");
                while (time < maxTime)
                {
                    UpdateAnts(ants, pheromones, dists);
                    UpdatePheromones(pheromones, ants, dists);

                    int[] currBestTrail = AntColonyProgram.BestTrail(ants, dists);
                    double currBestLength = Length(currBestTrail, dists);
                   // if (currBestLength < bestLength || currBestLength > bestLength || currBestLength == bestLength)
                    if (currBestLength < bestLength)
                    {
                        bestLength = currBestLength;
                        bestTrail = currBestTrail;
                        Console.WriteLine("Новая длина " + bestLength.ToString("F1") + " На Итерации: " + time);
                    }
                    time += 1;
                }

                Console.WriteLine("\nTime complete");

                Console.WriteLine("\nНайден лучший маршрут:");
                Display(bestTrail);
                Console.WriteLine("\nДлина наилучшего найденного маршрута: " + bestLength.ToString("F1"));

                Console.WriteLine("\nКонец\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }
        // Main

        // --------------------------------------------------------------------------------------------

        private static int[][] InitAnts(int numAnts, int numCities)
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++)
            {
                int start = random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }

        private static int[] RandomTrail(int start, int numCities)
        {
            // помощник для InitAnts
            int[] trail = new int[numCities];

            // последовательный
            for (int i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Тасование Фишера-Йетса
            for (int i = 0; i <= numCities - 1; i++)
            {
                int r = random.Next(i, numCities);
                int tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start);
            // помещаем начало с [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private static int IndexOfTarget(int[] trail, int target)
        {
            // помощник для RandomTrail
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception("Target not found in IndexOfTarget");
        }

        private static double Length(int[] trail, int[][] dists)
        {
            // общая длина тропы
            double result = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], dists);
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private static int[] BestTrail(int[][] ants, int[][] dists)
        {
            // лучший маршрут имеет наименьшую общую длину
            double bestLength = Length(ants[0], dists);
            int idxBestLength = 0;
            for (int k = 1; k <= ants.Length - 1; k++)
            {
                double len = Length(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numCities = ants[0].Length;
            // МГНОВЕННОЕ ПРИМЕЧАНИЕ VB: локальная переменная bestTrail была переименована, поскольку Visual Basic не допускает использование локальных переменных с тем же именем, что и их включающая функция или свойство:
            int[] bestTrail_Renamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }

        // --------------------------------------------------------------------------------------------

        private static double[][] InitPheromones(int numCities)
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // иначе первый вызов UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => выбрасывает
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                int start = random.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

        private static int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i <= numCities - 2; i++)
            {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

        private static int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // для муравья k (с visit []), в узле X, какой следующий узел в следе?
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // рассмотрите возможность установки cumul [cuml.Length-1] на 1.00
            }

            double p = random.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private static double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // для муравья k, расположенного в nodeX, с visit [], возвращаем вероятность перемещения в каждый город
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            // включает cityX и посещенные города
            double sum = 0.0;
            // сумма всех тауэтов
            // i - соседний город
            for (int i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // вероятность перехода к себе равна 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // вероятность переезда в посещаемый город равна 0
                }
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
                    // может быть огромным, когда феромон [] [] большой
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // большая проблема, если сумма = 0,0
            }
            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double length = AntColonyProgram.Length(ants[k], dists);
                        // длина k-го следа муравья
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = (Q / length);
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private static bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // городX и городY прилегают друг к другу в trail []?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }
            else if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            else if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == cityY)
            {
                return true;
            }
            else if (trail[idx + 1] == cityY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------

        private static int[][] MakeGraphDistances(int numCities)
        {
            int[][] dists = new int[numCities][];
            for (int i = 0; i <= dists.Length - 1; i++)
            {
                dists[i] = new int[numCities];
            }
            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    int d = random.Next(1, 40);
                    // [1,8]
                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }
            return dists;
        }

        private static double Distance(int cityX, int cityY, int[][] dists)
        {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------

        private static void Display(int[] trail)
        {
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                Console.Write(trail[i] + " ");
                if (i > 0 && i % 20 == 0)
                {
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("");
        }


        private static void ShowAnts(int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                Console.Write(i + ": [ ");

                for (int j = 0; j <= 3; j++)
                {
                    Console.Write(ants[i][j] + " ");
                }

                Console.Write(". . . ");

                for (int j = ants[i].Length - 4; j <= ants[i].Length - 1; j++)
                {
                    Console.Write(ants[i][j] + " ");
                }

                Console.Write("] len = ");
                double len = Length(ants[i], dists);
                Console.Write(len.ToString("F1"));
                Console.WriteLine("");
            }
        }

        private static void Display(double[][] pheromones)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                Console.Write(i + ": ");
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    Console.Write(pheromones[i][j].ToString("F4").PadLeft(8) + " ");
                }
                Console.WriteLine("");
            }

        }

    }
   

}

