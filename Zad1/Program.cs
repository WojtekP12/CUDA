using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;

namespace Zad1
{
    class Program
    {
        static string csvFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\";

        static int numberOfElements = 65536000;
        static int numberOfSortedElements = 256000;
        
        static int[] randomValues = new int[numberOfElements];
        static int[] sortedValues = new int[numberOfSortedElements];

        static Dictionary<int, int> randomValuesTimes = new Dictionary<int, int>();
        static Dictionary<int, int> sortedValuesTimes = new Dictionary<int, int>();

        static void Main(string[] args)
        {
            Console.WriteLine("Filling arrays...");
            Console.WriteLine();
            FillDataArrays();

            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, randomValues);
                size = s.Length;
            }

            Thread t = new Thread(new ParameterizedThreadStart(MeasureAlgorithmsTime), 104857600);

            Console.WriteLine("Sorting...");
            Console.WriteLine("Times:");
            Console.WriteLine();

            Console.WriteLine("Sorted values [Optimistic]: ");
            t.Start(Complexity.Optimistic);
            t.Join();

            Console.WriteLine();
            Console.WriteLine("Random values [Pessimistic]: ");
            t = new Thread(new ParameterizedThreadStart(MeasureAlgorithmsTime), 104857600);
            t.Start(Complexity.Pessimistic);
            t.Join();

            SaveDataToCSVFile("data");
            Console.WriteLine();
            Console.WriteLine("press any key");
            Console.ReadLine();
        }

        private static void FillDataArrays()
        {
            Random randomizer = new Random();

            for (int i = 0; i < numberOfElements; i++)
            {
                randomValues[i] = randomizer.Next(0, 100);
            }

            for (int i = 0; i < numberOfSortedElements; i++)
            {
                sortedValues[i] = i;
            }
        }

        private static void MeasureAlgorithmsTime(object complexity)
        {
            Complexity complex= (Complexity)complexity;

            switch(complex)
            {
                case Complexity.Optimistic:
                    Measure(randomValues, sortedValuesTimes);
                    break;
                case Complexity.Pessimistic:
                    Measure(sortedValues, randomValuesTimes);
                    break;
            }
        }

        private static void Measure(int[] data, Dictionary<int, int> times)
        {
            int startNumberOfElements = 100;
            while (startNumberOfElements <= data.Length)
            {
                int[] dataToSort = data.Take(startNumberOfElements).ToArray();
                int miliSeconds = AlgorithmTime(dataToSort);
                times.Add(startNumberOfElements, miliSeconds);
                Console.WriteLine("elements: " + startNumberOfElements + " seconds: " + miliSeconds);

                startNumberOfElements *= 2;
            };
        }

        private static int AlgorithmTime(int[] data)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            QuickSort(data, 0, data.Length - 1);
            stopwatch.Stop();

            return (int)stopwatch.ElapsedMilliseconds;
        }

        public static void QuickSort(int[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                if (pivot > 1)
                {
                    QuickSort(arr, left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    QuickSort(arr, pivot + 1, right);
                }
            }

        }

        public static int Partition(int[] arr, int left, int right)
        {
            int pivot = arr[left];
            while (true)
            {

                while (arr[left] < pivot)
                {
                    left++;
                }

                while (arr[right] > pivot)
                {
                    right--;
                }

                if (left < right)
                {
                    if (arr[left] == arr[right]) return right;

                    int temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;
                }
                else
                {
                    return right;
                }
            }
        }

        private static void SaveDataToCSVFile(string fileName)
        {
            fileName = csvFilePath + fileName;
            string txtfileName = fileName + ".txt";
            string csvFileName = fileName + ".csv";

            if (File.Exists(txtfileName))
            {
                File.Delete(txtfileName);
            }

            using (StreamWriter txtFile = new StreamWriter(txtfileName, true))
            {
                txtFile.WriteLine("OPTIMISTIC DATA");
                txtFile.WriteLine(String.Empty);

                foreach(var element in sortedValuesTimes)
                {
                    txtFile.WriteLine(element.Key+","+element.Value);
                }
                txtFile.WriteLine(String.Empty);
                txtFile.WriteLine(String.Empty);
                txtFile.WriteLine("PESSIMISTIC DATA");
                txtFile.WriteLine(String.Empty);

                foreach (var element in randomValuesTimes)
                {
                    txtFile.WriteLine(element.Key + "," + element.Value);
                }
            }

            if (File.Exists(csvFileName))
            {
                File.Delete(csvFileName);
            }

            File.Move(txtfileName, csvFileName);
        }
    }
}
