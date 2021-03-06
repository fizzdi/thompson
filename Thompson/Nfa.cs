﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Thompson
{
    public class NFA
    {
        private string _postfixRE;
        private string _infixRE;

        public string PostixRegularExpression
        {
            get
            {
                return _postfixRE;
            }
        }

        public string InfixRegularExpression
        {
            get
            {
                return _infixRE;
            }
        }

        public List<Char> alphabet;
        public List<List<List<int>>> grid;
        Random rnd = new Random();

        private static bool checkRegularBracketSequence(String str)
        {
            int balance = 0;
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '(': balance++; break;
                    case ')': balance--; break;
                }
                if (balance < 0) break;
            }
            return balance == 0;
        }

        public static bool NormalizeRE(String nonNormalizedString, out String NormalizedString)
        {
            NormalizedString = "";
            if (nonNormalizedString.Length < 1)
                return false; //"короткое регулярное выражение";

            NormalizedString = nonNormalizedString.Replace(" ", String.Empty);

            for (int i = 1; i < NormalizedString.Length; ++i)
            {
                char currentChar = NormalizedString[i];
                char previousChar = NormalizedString[i - 1];

                if (
                        (previousChar == '(') || (previousChar == '|') ||
                        (currentChar == '|') || (currentChar == ')') ||
                        (currentChar == '*') || (currentChar == '.') ||
                        ((currentChar == '(') && !(i > 0 && (previousChar != '(') && (previousChar != '|') && (previousChar != '.')))
                    )
                    continue;

                NormalizedString = NormalizedString.Insert(i, ".");
                i++;
            }
            return true;
        }

        public static string infix2postfix(String infixRE, out List<Char> alphabet)
        {
            alphabet = new List<Char>() { 'ε' };
            String postfixRE = String.Empty;
            Stack<Char> operationStack = new Stack<Char>();
            for (int i = 0; i < infixRE.Length; i++)
            {
                switch (infixRE[i])
                {
                    //если в стеке нет операций или верхним элементом стека является открывающая скобка 
                    //или новая операции имеет больший приоритет, чем верхняя операции в стеке, операции кладётся в стек;
                    //иначе
                    //если новая операция имеет меньший или равный приоритет, чем верхняя операции в стеке, то операции, 
                    //находящиеся в стеке, до ближайшей открывающей скобки или до операции с приоритетом меньшим, 
                    //чем у новой операции, перекладываются в формируемую запись, а новая операции кладётся в стек.
                    case '*':
                        while ((operationStack.Count != 0) && (operationStack.Peek() != '(') && (operationStack.Peek() != '.'))
                            postfixRE += operationStack.Pop();
                        operationStack.Push(infixRE[i]);
                        break;

                    case '.':
                        while ((operationStack.Count != 0) && (operationStack.Peek() != '(') && (operationStack.Peek() != '|'))
                            postfixRE += operationStack.Pop();
                        operationStack.Push(infixRE[i]);
                        break;

                    case '|':
                        while ((operationStack.Count != 0) && (operationStack.Peek() != '('))
                            postfixRE += operationStack.Pop();
                        operationStack.Push(infixRE[i]);
                        break;

                    case '(':
                        operationStack.Push(infixRE[i]);
                        break;

                    case ')':
                        while ((operationStack.Count != 0) && (operationStack.Peek() != '('))
                            postfixRE += operationStack.Pop();
                        if ((operationStack.Count != 0) && (operationStack.Peek() == '('))
                            operationStack.Pop();
                        break;

                    default:
                        postfixRE += infixRE[i];
                        if (!alphabet.Contains(infixRE[i]))
                            alphabet.Add(infixRE[i]);
                        break;
                }
            }
            while (operationStack.Count != 0)
                postfixRE += operationStack.Pop();
            return postfixRE;
        }

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

        public NFA(string nonNormalizedInfixRE)
        {
            if (nonNormalizedInfixRE.Length == 0)
            {
                throw new Exception("Отсутствует регулярное выражение");
            }

            if (!checkRegularBracketSequence(nonNormalizedInfixRE))
            {
                throw new Exception("Ошибка правильной скобочной последовательности");
            }

            if (nonNormalizedInfixRE[0] == '*')
            {
                throw new Exception("Выражение начинается с действия");
            }

            if (!NormalizeRE(nonNormalizedInfixRE, out _infixRE))
            {
                throw new Exception("Ошибка нормализации регулярного выражения");
            }

            _postfixRE = infix2postfix(_infixRE, out alphabet);
        }

        static private List<List<List<int>>> createEmptyGraph(int num_states, int num_alphabet)
        {
            List<List<List<int>>> result = new List<List<List<int>>>();
            for (int i = 0; i < num_states; ++i)
            {
                result.Add(new List<List<int>>());
                for (int j = 0; j < num_alphabet; ++j)
                    result[i].Add(new List<int>());
            }

            return result;
        }

        public void build(TextBox txb)
        {
            txb.Text = "";
            Stack<List<List<List<int>>>> transitionFunction = new Stack<List<List<List<int>>>>();

            for (int i = 0; i < _postfixRE.Length; ++i)
            {
                int indexInAlphabet = alphabet.IndexOf(_postfixRE[i]);
                switch (_postfixRE[i])
                {
                    case '*':
                        {
                            List<List<List<int>>> last = transitionFunction.Pop();
                            List<List<List<int>>> currentGraph = createEmptyGraph(2 + last.Count, alphabet.Count);
                            currentGraph[0][0].Add(1);
                            for (int ii = 0; ii < last.Count; ++ii)
                            {
                                for (int jj = 0; jj < last[ii].Count; ++jj)
                                {
                                    for (int kk = 0; kk < last[ii][jj].Count; ++kk)
                                    {
                                        currentGraph[ii + 1][jj].Add(last[ii][jj][kk] + 1);
                                    }
                                }
                            }
                            currentGraph[0][0].Add(1 + last.Count);
                            currentGraph[last.Count][0].Add(1);
                            currentGraph[last.Count][0].Add(1 + last.Count);
                            transitionFunction.Push(currentGraph);
                            print(currentGraph, txb);
                        }
                        break;

                    case '.':
                        {
                            List<List<List<int>>> right = transitionFunction.Pop();
                            List<List<List<int>>> left = transitionFunction.Pop();
                            List<List<List<int>>> currentGraph = createEmptyGraph(left.Count + right.Count - 1, alphabet.Count);
                            for (int ii = 0; ii < left.Count; ++ii)
                                for (int jj = 0; jj < left[ii].Count; ++jj)
                                    for (int kk = 0; kk < left[ii][jj].Count; ++kk)
                                        currentGraph[ii][jj].Add(left[ii][jj][kk]);
                            for (int ii = 0; ii < right.Count; ++ii)
                                for (int jj = 0; jj < right[ii].Count; ++jj)
                                    for (int kk = 0; kk < right[ii][jj].Count; ++kk)
                                        currentGraph[ii + left.Count - 1][jj].Add(right[ii][jj][kk] + left.Count - 1);
                            transitionFunction.Push(currentGraph);
                            print(currentGraph, txb);
                        }
                        break;

                    case '|':
                        {
                            List<List<List<int>>> right = transitionFunction.Pop();
                            List<List<List<int>>> left = transitionFunction.Pop();
                            List<List<List<int>>> currentGraph = createEmptyGraph(left.Count + right.Count + 2, alphabet.Count);

                            currentGraph[0][0].Add(1);
                            currentGraph[0][0].Add(left.Count + 1);
                            for (int ii = 0; ii < left.Count; ++ii)
                                for (int jj = 0; jj < left[ii].Count; ++jj)
                                    for (int kk = 0; kk < left[ii][jj].Count; ++kk)
                                        currentGraph[ii + 1][jj].Add(left[ii][jj][kk] + 1);

                            for (int ii = 0; ii < right.Count; ++ii)
                                for (int jj = 0; jj < right[ii].Count; ++jj)
                                    for (int kk = 0; kk < right[ii][jj].Count; ++kk)
                                        currentGraph[ii + left.Count + 1][jj].Add(right[ii][jj][kk] + left.Count + 1);

                            currentGraph[left.Count][0].Add(currentGraph.Count - 1);
                            currentGraph[right.Count + left.Count][0].Add(currentGraph.Count - 1);
                            transitionFunction.Push(currentGraph);
                            print(currentGraph, txb);
                        }
                        break;

                    default:
                        {
                            List<List<List<int>>> currentGraph = createEmptyGraph(2, alphabet.Count);
                            currentGraph[0][indexInAlphabet].Add(1);
                            transitionFunction.Push(currentGraph);
                            print(currentGraph, txb);
                        }
                        break;
                }
            }
            grid = transitionFunction.Pop();
            print(grid, txb);
        }

        void print(List<List<List<int>>> tabl, TextBox txb)
        {
            System.Text.StringBuilder str_build = new System.Text.StringBuilder();
            using (StringWriter sw = new StringWriter(str_build))
            {
                sw.WriteLine("-------------------------------------------");
                //Первый отступ
                int fpad = 5;
                //размер стобца
                int colsize = 0;

                List<List<string>> string_vertex = new List<List<string>>();
                for (int i = 0; i < tabl.Count; ++i)
                {
                    string_vertex.Add(new List<string>());
                    for (int j = 0; j < tabl[i].Count; ++j)
                    {
                        string_vertex[i].Add("");
                        for (int k = 0; k < tabl[i][j].Count; ++k)
                        {
                            if (k != 0)
                                string_vertex[i][j] += ",";
                            string_vertex[i][j] += "S" + tabl[i][j][k];
                            colsize = Math.Max(colsize, string_vertex[i][j].Length);
                        }
                    }
                }

                string str = string.Format(string.Format("{0}{1}{2}", "{0,", fpad, "}|"), "");

                for (int i = 0; i < alphabet.Count; i++)
                {
                    int lf = (colsize + 1) / 2;
                    int rf = (colsize + 1) - lf;
                    str += string.Format(string.Format("{0}{1}{2}", "{0,", lf, "}"), "");
                    str += alphabet[i];
                    str += string.Format(string.Format("{0}{1}{2}", "{0,", rf, "}|"), "");
                }
                sw.WriteLine(str);

                for (int i = 0; i < string_vertex.Count; ++i)
                {
                    string row = String.Format(String.Format("{0}{1}{2}", "{0,", -fpad, "}|"), "S" + i);
                    for (int j = 0; j < string_vertex[i].Count; ++j)
                    {
                        row += string.Format(string.Format("{0}{1}{2}", "{0}{1,", colsize, "}{2}|"), "{", string_vertex[i][j], "}");
                    }
                    sw.WriteLine(row);
                }
            }
            txb.Text += str_build.ToString();
        }

        public bool make(string st)
        {
            int m = alphabet.Count;
            int n;
            Stack s = new Stack();//входной стек
            Stack r = new Stack();//выходной стек
            for (int i = 0; i < grid[0][0].Count; i++)
                s.Push(grid[0][0][i]);
            s.Push(0);
            for (int i = 0; i < st.Length; i++)
            {
                if ((n = alphabet.IndexOf(st[i])) < 0) return false;
                while (s.Count > 0)
                {
                    int state = (int)s.Pop();//состояние
                    for (int j = 0; j < grid[state][n].Count; j++)
                        addStates(r, grid[state][n][j]);

                    for (int j = 0; j < grid[state][0].Count; ++j)
                        addStates(r, grid[state][0][j]);
                }
                while (r.Count > 0)
                    s.Push(r.Pop());
            }
            return (s.Contains(grid.Count - 1));
        }

        private void addStates(Stack r, int cur_state)
        {
            if (!r.Contains(cur_state)) r.Push(cur_state);
            for (int i = 0; i < grid[cur_state][0].Count; ++i)
                addStates(r, grid[cur_state][0][i]);
        }
    }
}
