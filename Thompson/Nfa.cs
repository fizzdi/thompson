using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Thompson
{

    public class Input
    {
        public string[] lines;//для хранения файла input 
        public String init()
        {
            string path = Directory.GetCurrentDirectory() + '/';
            string file = path + "input.txt";
            if (!File.Exists(file))
                return "файл input.txt не найден";
            File.OpenRead(file);
            lines = File.ReadAllLines(file, Encoding.Default);
            if (lines.Length < 1) return "короткий файл";
            if (lines[0].Length < 1) return "короткое регулярное выражение";
            for (int j = 0; j < lines.Length; ++j)
                lines[j] = lines[j].Replace(" ", "");
            int i = 0;
            while (++i < lines[0].Length)
            {
                char cur = lines[0][i];
                char pred = lines[0][i - 1];

                if ((pred == '(') || (pred == '|') || (cur == '|') || (cur == ')') || (cur == '*') || (cur == '.') || ((cur == '(') && !(i > 0 && (pred != '(') && (pred != '|') && (pred != '.'))))
                    continue;

                lines[0] = lines[0].Insert(i, ".");
                i++;
            }
            if (lines.Length < 2) return "короткий файл";
            return "OK";
        }
    }
    public class Nfa
    {
        public string ex;//собственно выражение
        public ArrayList abc;
        public ArrayList[] tabl;
        Random rando = new Random();

        //инициализация массива размером _size состоящего из элементов null
        static private ArrayList initArrayList(int _size)
        {
            ArrayList res = new ArrayList();
            for (int i = 0; i < _size; ++i)
            {
                res.Add(null);
            }
            return res;
        }

        public Nfa(string ex_)
        {
            Console.WriteLine(ex_);
            abc = new ArrayList();
            Stack st = new Stack();
            st.Push(' ');
            ex = "";
            //string ch;
            for (int i = 0; i < ex_.Length; i++)
            {
                switch (ex_[i])
                {
                    //если в стеке нет операций или верхним элементом стека является открывающая скобка 
                    //или новая операции имеет больший приоритет, чем верхняя операции в стеке, операции кладётся в стек;
                    //иначе
                    //если новая операция имеет меньший или равный приоритет, чем верхняя операции в стеке, то операции, 
                    //находящиеся в стеке, до ближайшей открывающей скобки или до операции с приоритетом меньшим, 
                    //чем у новой операции, перекладываются в формируемую запись, а новая операции кладётся в стек.
                    case '*'://иттерация
                        while (!((st.Count < 2) | (st.Peek().ToString() == "(") | (st.Peek().ToString() != "*")))
                            ex += st.Pop().ToString();
                        st.Push(ex_[i]);
                        break;
                    case '.'://Конкатенация
                        while (!((st.Count < 2) | (st.Peek().ToString() == "(") | (st.Peek().ToString() == "|")))
                            ex += st.Pop().ToString();
                        st.Push(ex_[i]);
                        break;
                    case '|'://объединение
                        while (!((st.Count < 2) | (st.Peek().ToString() == "(")))
                            ex += st.Pop().ToString();
                        st.Push(ex_[i]);
                        break;
                    case ')':
                        while (st.Peek().ToString() != "(")
                            ex += st.Pop().ToString();
                        st.Pop();
                        break;
                    case '(':
                        st.Push(ex_[i]);
                        break;
                    default:
                        ex += ex_[i];
                        if (!abc.Contains(ex_[i])) abc.Add(ex_[i]);
                        break;
                }
                Console.WriteLine(ex);
            }
            while (st.Count > 1) ex += st.Pop();
            st.Pop();
            String s = "";
            for (int j = 0; j < abc.Count; j++) s += " " + abc[j].ToString();
            Console.WriteLine(ex + "\n" + s);
        }
        public void build()
        {
            Stack st = new Stack();
            int m = abc.Count + 1;
            ArrayList[] s;
            ArrayList[] r;
            ArrayList[] t;
            ArrayList q;
            for (int i = 0; i < ex.Length; i++)
            {
                int n = abc.IndexOf(ex[i]);
                switch (ex[i])
                {
                    case '*'://иттерация
                        s = (ArrayList[])st.Pop();//исходный
                        r = new ArrayList[m];//здесь живут переходы
                        for (int j = 0; j < m; j++)//перебор по символам алфавита
                        {
                            r[j] = initArrayList(s[j].Count + 2);
                            for (int k = 0; k < s[j].Count; k++)//перебор по состояниям
                            {
                                r[j][k + 1] = s[j][k];//включение исходного в итог
                                if (r[j][k + 1] != null)
                                {// исправление переходов из-за смены номеров состояний у первого
                                    q = (ArrayList)r[j][k + 1];
                                    for (int l = 0; l < q.Count; l++)
                                        q[l] = (int)q[l] + 1;
                                }
                            }
                        }
                        q = new ArrayList();// большинство переходов = null
                        q.Add(1);
                        q.Add(s[m - 1].Count + 1);
                        r[m - 1][0] = q;
                        q = new ArrayList(q);
                        r[m - 1][s[m - 1].Count] = q;//прописали переход из 0 и предпоследнего в 1 и в конец по пустому символу

                        print(r);
                        st.Push(r);// 
                        break;
                    case '.'://конкатенация
                        t = (ArrayList[])st.Pop();//второй
                        s = (ArrayList[])st.Pop();//первый
                        r = new ArrayList[m];//здесь живут переходы
                        for (int j = 0; j < m; j++)//перебор по символам алфавита
                        {
                            r[j] = initArrayList(s[j].Count + t[j].Count - 1);
                            for (int k = 0; k < s[j].Count - 1; k++)//перебор по состояниям
                                r[j][k] = s[j][k];//включение первого в итог
                            for (int k = s[j].Count - 1; k < r[j].Count; k++)//перебор по состояниям
                            {
                                r[j][k] = t[j][k + 1 - s[j].Count];//включение второго в итог

                                if (r[j][k] != null)
                                {// исправление переходов из-за смены номеров состояний у второго
                                    q = (ArrayList)r[j][k];
                                    for (int l = 0; l < q.Count; l++)
                                        q[l] = (int)q[l] + s[j].Count - 1;
                                }
                            }
                        }
                        print(r);
                        st.Push(r);// 
                        break;
                    case '|'://объединение
                        t = (ArrayList[])st.Pop();//второй
                        s = (ArrayList[])st.Pop();//первый
                        r = new ArrayList[m];//здесь живут переходы
                        for (int j = 0; j < m; j++)//перебор по символам алфавита
                        {
                            r[j] = initArrayList(s[j].Count + t[j].Count + 2);
                            for (int k = 1; k < s[j].Count + 1; k++)//перебор по состояниям
                            {
                                r[j][k] = s[j][k - 1];//включение первого в итог
                                if (r[j][k] != null)
                                {// исправление переходов из-за смены номеров состояний у первого
                                    q = (ArrayList)r[j][k];
                                    for (int l = 0; l < q.Count; l++)
                                        q[l] = (int)q[l] + 1;
                                }
                            }
                            for (int k = s[j].Count + 1; k < r[j].Count - 1; k++)//перебор по состояниям
                            {
                                r[j][k] = t[j][k - 1 - s[j].Count];//включение второго в итог
                                if (r[j][k] != null)
                                {// исправление переходов из-за смены номеров состояний у второго
                                    q = (ArrayList)r[j][k];
                                    for (int l = 0; l < q.Count; l++)
                                        q[l] = (int)q[l] + s[j].Count + 1;
                                }
                            }
                        }
                        q = new ArrayList();// большинство переходов = null
                        q.Add(1);
                        q.Add(s[m - 1].Count + 1);
                        r[m - 1][0] = q;//прописали переход из 0 в 1 и в начало t по пустому символу
                        q = new ArrayList();
                        q.Add(r[m - 1].Count - 1);
                        r[m - 1][s[m - 1].Count] = q;//прописали переход из s в конец по пустому символу
                        q = new ArrayList();// большинство переходов = null
                        q.Add(r[m - 1].Count - 1);
                        r[m - 1][r[m - 1].Count - 2] = q;//прописали переход из t в конец по пустому символу
                        print(r);
                        st.Push(r);// 
                        break;
                    default:
                        r = new ArrayList[m];//здесь живут переходы
                        for (int j = 0; j < m; j++)//перебор по символам алфавита
                        {
                            r[j] = initArrayList(2);
                        }
                        q = new ArrayList();// большинство переходов = null
                        q.Add(1);
                        r[n][0] = q;//прописали переход из 0 в 1 по символу алфавита
                        print(r);
                        st.Push(r);// сложили в стек
                        break;
                }
            }
            //            Console.WriteLine("стек = "+st.Count);
            tabl = (ArrayList[])st.Pop();
            /*            for(int i=0;i<tabl[m-1].Count;i++)// каждому состоянию для пустого символа если нет переходов добавляем замыкание на себя
                            if(tabl[m - 1][i]==null)
                            {
                                q = new ArrayList();
                                q.Add(i);
                                tabl[m - 1][i] = q;
                            }
                        print(tabl);*/
        }
        public string make(string st)
        {
            int m = abc.Count;
            int n;
            Stack s = new Stack();//входной стек
            Stack r = new Stack();//выходной стек
            ArrayList q = (ArrayList)tabl[m][0];//список состояний достижимых из стартового при пустом переходе
            if (q != null)
                for (int i = 0; i < q.Count; i++)
                    s.Push(q[i]);
            else s.Push(0);
            print_s(s);
            for (int i = 0; i < st.Length; i++)
            {
                if ((n = abc.IndexOf(st[i])) < 0) return "  NO, за пределами алфавита";
                while (s.Count > 0)
                {
                    int state = (int)s.Pop();//состояние
                    if (tabl[n][state] != null)//в ячеке таблицы для обрабатывамого состояния + символа не null
                    {
                        q = (ArrayList)tabl[n][state];
                        for (int j = 0; j < q.Count; j++)
                            add(r, (int)q[j]);
                    }// move 

                    if (tabl[tabl.Length - 1][state] != null)
                    {
                        var tab = tabl[tabl.Length - 1][state] as ArrayList;
                        for (int j = 0; j < tab.Count; ++j)
                            add(r, (int)tab[j]);
                    }
                }
                print_s(r);

                while (r.Count > 0)
                    s.Push(r.Pop());
            }
            if (s.Contains(tabl[0].Count - 1)) return "  YES";
            return "  NO";
        }
        void add(Stack r, int s)
        {
            if (!r.Contains(s)) r.Push(s);
            if (tabl[abc.Count][s] != null)
            {
                ArrayList q = (ArrayList)tabl[abc.Count][s];
                for (int j = 0; j < q.Count; j++)
                    add(r, (int)q[j]);
            }// move 
        }
        void print_s(Stack s)
        {
            string st = "стек ";
            for (int i = 0; i < tabl[0].Count; i++)
                if (s.Contains(i)) st += " " + i;
            Console.WriteLine(st);
        }
        //.Remove(0,1)
        void print(ArrayList[] tabl)
        {
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------");
            //Первый отступ
            int fpad = 20;
            //размер стобца
            int colsize = 10;

            string str = string.Format(string.Format("{0}{1}{2}", "{0,", fpad, "}"), " ");
            for (int i = 0; i < abc.Count; i++)
                str += string.Format(string.Format("{0}{1}{2}", "{0,", colsize, "}"), abc[i]);
            Console.WriteLine(str);
            for (int i = 0; i < tabl[0].Count; i++)
            {
                str = "S" + i + " = {S" + i;
                if (tabl[tabl.Length - 1][i] != null)
                {
                    var ctab = (tabl[tabl.Length - 1][i] as ArrayList);
                    for (int j = 0; j < ctab.Count; ++j)
                    {
                        str += ", S" + (int)(ctab[j]);
                    }
                }
                str += "} ";
                str = str.PadRight(fpad);
                for (int k = 0; k < abc.Count; k++)
                    if (tabl[k][i] == null)
                        str += string.Format(string.Format("{0}{1}{2}", "{0,", colsize, "}"), "null");
                    else
                    {
                        ArrayList q = (ArrayList)tabl[k][i];
                        string strg = "";
                        for (int j = 0; j < q.Count; j++)
                        {
                            if (j > 0)
                                strg += ", S" + (int)q[j];
                            else
                                strg += "S" + (int)q[j];
                        }
                        str += strg.PadLeft(colsize);
                    }
                Console.WriteLine(str);
            }
        }
        /*        public string random()
                {
                    string s = "";
                    int len = rando.Next(8);
                    for (int i = 0; i < len; i++)
                        s += abc[rando.Next(abc.Count)];
                    return s;
                }*/
    }
}
