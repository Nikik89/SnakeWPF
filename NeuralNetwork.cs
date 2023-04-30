using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeWPF
{
    public class NeuralNetwork
    {
        // Входные данные
        double[] inputs;

        // Выходные данные
        double[] outputs;

        // Веса связей
        double[,] weights;
        

        // Конструктор
        public NeuralNetwork(int inputSize, int outputSize)
        {
            inputs = new double[inputSize];
            outputs = new double[outputSize];
            weights = new double[inputSize, outputSize];
            
            // Инициализация весов случайными значениями

            Random rnd = new Random();
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < outputSize; j++)
                {
                    weights[i, j] = rnd.NextDouble();
                }
            }
        }

        // Прямое распространение
        public double[] FeedForward(double[] inputs)
        {
            this.inputs = inputs;

            // Вычисление выходных значений
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < inputs.Length; j++)
                {
                    outputs[i] += inputs[j] * weights[j, i];
                }
            }

            return outputs;
        }

        // Обратное распространение
        public void BackPropagation(double[] expected)
        {
            
            // Вычисление ошибки для каждого выходного нейрона
            double[] errors = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                errors[i] = expected[i] - outputs[i];
            }

            // Вычисление дельт для каждого веса
            double[,] deltas = new double[inputs.Length, outputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                for (int j = 0; j < outputs.Length; j++)
                {
                    deltas[i, j] = errors[j] * inputs[i];
                }
            }

            // Обновление весов

            for (int i = 0; i < inputs.Length; i++)
            {
                for (int j = 0; j < outputs.Length; j++)
                {
                    weights[i, j] += deltas[i, j];
                }
            }
            using (FileStream fs = new FileStream("test1.txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach(double weight in weights)
                    {
                        sw.WriteLine(weight);
                    }
                }
            }
        }
    }
}
