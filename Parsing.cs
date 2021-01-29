using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace SimbirSoftTask
{
    class Parsing
    {
        public string PathBodyPage { get => "BodyPage"; }
        public string PathFileForSaveHTML
        {
            get => pathFileForSaveHTML;
            set
            {
                if(Uri.IsWellFormedUriString(value, UriKind.Relative))
                {
                    pathFileForSaveHTML = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        public string PathFileForSaveWords
        {
            get => pathFileForSaveWords;
            set
            {
                if (Uri.IsWellFormedUriString(value, UriKind.Relative))
                {
                    pathFileForSaveWords = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        public string PathWebSite 
        {
            get => pathWebSite;
            set
            {
                if(Uri.IsWellFormedUriString(value,UriKind.Absolute))
                {
                    pathWebSite = value;
                }
                else
                {
                    throw new FormatException("Неверный формат ссылки на сайт");
                }
            }
        }


        public Parsing(string pathfile,string pathForSaveHTML,string pathForSaveWords)
        {

                PathWebSite = pathfile;
                PathFileForSaveHTML = pathForSaveHTML;
                PathFileForSaveWords = pathForSaveWords;


                russinalphabet = Enumerable.Range(0, 32).Select((x, i) => (char)('а' + i)).ToArray();
                englishalphabet = Enumerable.Range(0, 32).Select((x, i) => (char)('a' + i)).ToArray();

                dictionary = new Dictionary<char, List<(string word, int count)>>();

                for (int i = 0; i < russinalphabet.Length; i++)
                {
                    dictionary.Add(russinalphabet[i], new List<(string word, int count)>());
                }

                



        }

        /// <summary>
        /// Основной метод класса,парсит слова из текстового файла с HTML разметкой.Выводит слова на экран
        /// </summary>
        /// <param name="pathFile"></param>
        public void StartParsing() 
        {
            if (DownloadWebSite(out Exception e))
            {
                string page = null;
                // Считывает текстовый документ с HTML и передаем значение строки в page
                using (StreamReader sr = new StreamReader(PathFileForSaveHTML))
                {
                    page = sr.ReadToEnd().ToLower();

                }
                //Создаем и передает текстовму документу весь тег Body
                using (StreamWriter sw = new StreamWriter(PathBodyPage, false))
                {
                    sw.Write(LeaveOnlyBodyTag(page));
                }
                //Считываем документ с тегом Body
                using (var sr = new StreamReader(PathBodyPage))
                {
                    //Передаем значения тега Body в bodypage
                    string bodypage = sr.ReadToEnd();
                    // Создаем и записываем в текстовый документ слова 
                    using (var sw = new StreamWriter(PathFileForSaveWords, false))
                    {
                        //Получаем список слов 
                        var words = Cutting(bodypage);
                        // Добавляем слова с словарь слов
                        foreach (var word in words)
                        {
                            AddWordAlphaBet(word);
                        }

                        //Выводим слова на консоль
                        foreach (var word in dictionary)
                        {
                            if (word.Value.Count != 0)
                            {
                                foreach (var item in word.Value)
                                {
                                    Console.WriteLine($"{item.word} - {item.count}");
                                    sw.WriteLine($"{item.word} - {item.count}");
                                }
                            }
                        }
                    }
                }
            }
            else
            {

                Console.WriteLine("Что-то пошло не так");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Оставляет из всего Html документа только body тег
        /// </summary>
        /// <param name="htmlpage">Ссылка на текстовый файл HTML</param>
        /// <returns>Возвращает строку с тегом body</returns>
        private string LeaveOnlyBodyTag(string htmlpage) 
        {
            (int start, int end) body = FindBodyIndex(htmlpage);
            string bodypage = htmlpage.Substring(body.start, body.end - body.start);
            return bodypage;
        }
        private void AddWordAlphaBet(string word) 
        {
            var separation = word.Split(new char[] { ' ' });
            foreach(string newword in separation)
            {
                int i = 0;
                char firstletter = newword.ToLower()[0];
                if (russinalphabet.Contains(firstletter))
                {
                    while (i < dictionary[firstletter].Count)
                    {
                        if (newword.ToLower() == dictionary[firstletter][i].word.ToLower())
                        {
                            dictionary[firstletter][i] = (newword, dictionary[firstletter][i].count + 1);
                            break;
                        }
                        i++;
                    }
                    if (i == dictionary[firstletter].Count)
                    {
                        dictionary[firstletter].Add((newword, 1));
                    }
                }
            }

        }

        /// <summary>
        /// Список слов из тега Body
        /// </summary>
        /// <param name="bodypage">Ссылка на BodyPage.txt</param>
        /// <returns>Возвращает список из строк</returns>
        private List<string> Cutting(string bodypage) 
        {
            List<string> words = new List<string>();
            (int start, int end) index = (0,0);
            bool repeat = true;
            while (repeat)
            {
                index = FindWords(bodypage,index.end);  // 1 1642 1666 слово или словосочитание
                if (index.start != -1 && index.end != -1)
                {
                    string str = bodypage.Substring(index.start, index.end - index.start);
                    words.Add(WebUtility.UrlDecode(CleanUpString(str)));
                }
                else
                {
                    repeat = false;
                }
            }
            return words;

            
        }
        /// <summary>
        /// Находит индексы открывания и закрывания тега body
        /// </summary>
        /// <param name="bodypage"></param>
        /// <returns>(Начало,Конец)</returns>
        private (int start ,int end) FindBodyIndex(string htmlpage) 
        {
            int start = htmlpage.IndexOf("<body");
            int end = htmlpage.IndexOf("</body");
            
            return (start, end);
        }

        /// <summary>
        /// Находит слова в теге body
        /// </summary>
        /// <param name="bodypage">Ссылка на BodyPage.txt</param>
        /// <returns>(Начало,конец слова)</returns>       
        private (int start,int end) FindWords(string bodypage,int end) 
        {
            int newstart = end + 1;
            if(newstart < bodypage.Length) 
            {
                int index = newstart;
                int index1 = bodypage.IndexOf('"', index);
                int index2 = bodypage.IndexOf('>', index);
                while (true)
                {

                    int newend;
                    if (index1 != -1 && index1 < index2 && russinalphabet.Contains(char.ToLower(bodypage[index1 + 1])))
                    {
                        newstart = index1 + 1;
                        newend = newstart + 1;
                        while(newend < bodypage.Length && (russinalphabet.Contains(char.ToLower(bodypage[newend])) || bodypage[newend] == ' ')) 
                        {
                            newend++;
                        }
                        return(newstart, newend);

                    }
                    //else if(index2 != -1 && index1 > index2 && russinalphabet.Contains(char.ToLower(bodypage[index2 + 1])))
                    else if (index2 != -1 && index1 > index2 && CheckGoNext(index2,bodypage))
                    {
                        newstart = index2 + 1;
                        newend = newstart + 1;
                        while (newend < bodypage.Length && bodypage[newend] != '<')
                        {
                            newend++;
                        }
                        return (newstart, newend );
                    }
                    else
                    {
                        if(index1 == -1 || index2 == -1) 
                        {
                            return (-1, -1);
                            
                        }
                        else if(index1 > index2)
                        {
                            index = index2;
                            index1 = bodypage.IndexOf('"', index);
                            index2 = bodypage.IndexOf('>', index + 1);
                        }
                        else if (index1 < index2)
                        {
                            index = index1;
                            index1 = bodypage.IndexOf('"', index + 1);
                            index2 = bodypage.IndexOf('>', index);
                        }

                    }

                }
            }
            return (-1,-1);
            
        }     

        private bool CheckGoNext(int index,string bodytag) 
        {
            char mainchar = char.ToLower(bodytag[index]);
            switch (mainchar) 
            {
                case '>':
                    int newindex = index;
                    while(bodytag.Length > newindex && bodytag[newindex] != '<')
                    {
                        newindex++;
                    }
                    if(bodytag.Length < newindex)
                    {
                        return false;
                    }
                    else
                    {
                        while(newindex > index && !russinalphabet.Contains(bodytag[index + 1]))
                        {
                            index++;
                        }
                        return newindex > index; 
                    }
                //case '"':

                //    break;
                default:
                    return false;
                    break;
            }
        }

        private string CleanUpString(string str) 
        {
            List<string> words = new List<string>();
            int endindex = 0;
            int index = 0;
            while (str.Length > endindex && str.Length > index)
            {
                while (str.Length > index && !(russinalphabet.Contains(char.ToLower(str[index])) || englishalphabet.Contains(char.ToLower(str[index]))))
                {
                    index++;
                }
                endindex = index + 1;
                while (str.Length > endindex && (russinalphabet.Contains(char.ToLower(str[endindex])) || englishalphabet.Contains(char.ToLower(str[endindex]))))
                {
                    endindex++;
                }
                if (str.Length >= endindex && str.Length >= index)
                {
                    words.Add(str.Substring(index, endindex - index));
                    index = endindex + 1;
                }
            }
            string newstring = null;
            foreach(var word in words)
            {
                if (newstring == null)
                    newstring = word;
                else
                    newstring = newstring + ' ' + word;
            }
                return newstring;
        }

        private bool DownloadWebSite(out Exception e)
        {
            WebClient wc = new WebClient();
            try
            {
                Stream str = wc.OpenRead(PathWebSite);
                StreamReader streamReader = new StreamReader(str, Encoding.Default);
                using (StreamWriter sw = new StreamWriter(PathFileForSaveHTML, false))
                {
                    sw.WriteLine(WebUtility.HtmlDecode(streamReader.ReadToEnd()));
                }
                str.Close();
                streamReader.Close();

            }
            catch(Exception exp) 
            {
                e = exp;
                return false;
            }
            e = null;
            return true;
        }


        char[] russinalphabet;
        char[] englishalphabet;
        Dictionary<char, List<(string word, int count)>> dictionary;
        string pathWebSite;
        string pathFileForSaveHTML;
        string pathFileForSaveWords;
    }
}