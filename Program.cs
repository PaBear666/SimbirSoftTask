using System;
using System.Text.RegularExpressions;

namespace SimbirSoftTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать!");
            Console.WriteLine("Данная программа находит русские слова на странице и выводит их в консоль");
            Console.WriteLine("Для начала работы введите URL сайта");
            Console.WriteLine("Пример URL: https://www.simbirsoft.com");
                Console.Write("URL: ");
                string url = Console.ReadLine().ToLower();
                Console.WriteLine();
                Console.WriteLine("Введите ссылку на файл,где сохранить HTML сайта.");
                Console.WriteLine("Если файл отсутсвует,программа создаст его автоматически");
                Console.Write("URL FileHTML:");
                string pathHTML = Console.ReadLine().ToLower();
                Console.WriteLine();
                Console.WriteLine("Введите ссылку на файл,где сохранятся слова страницы сайта");
                Console.Write("URL FileWords:");
                string pathText = Console.ReadLine().ToLower();
                try
                {
                    Console.WriteLine("Скачивание пошло!");
                    Parsing parsing = new Parsing(url,pathHTML,pathText);
                    parsing.StartParsing();
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
        }
    }
}
