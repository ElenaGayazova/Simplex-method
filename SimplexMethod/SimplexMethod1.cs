using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplexMethod
{
    /*   Тип данных для указания, решается задача
     * на минимум или на максимум, */
    enum Zadacha { Max, Min  }
    /*    Тип решения задачи, существует решение 
     * или не существует,           */
    enum Solution { NotExists, Exists }

     /* Класс для решения задачи  симплекс - методом, */
    class SimplexMethod1
    {
        /*  Количество переменных в задаче,  */ 
        int n = 0;
        /*  Количество переменных с дополнительными переменными, */
        int n1 = 0;
        /*  Количество ограничений в системе ограничений,  */
        int m = 0;
        
        /* Матрица коэффициентов системы ограничений, */
        List<List<double>> a = new List<List<double>>();
        /* Вектор свободных членов системы ограничений, */
        List<double> b = new List<double>();
        /* Вектор коэффициентов целевой  функции,  */
        List<double> c = new List<double>();
        /*      Вектор коэффициентов целевой  функции 
         *  для базисных переменных, */
        List<double> Cb = new List<double>();
        /* Названия переменных, */
        List<string> name = new List<string>();
        /*  Номера базисных переменных, */
        List<int> basis = new List<int>();
        /*  Решение задачи, */
        List<double> x = new List<double>();
        /* Вектор оценок для определения оптимальности решения, */
        List<double> delta = new List<double>();
        /* Значение целевой функции, */
        double z = 0;
        /* Решение задачи на минимум или на максимум, */
        Zadacha f = Zadacha.Max;
        /* Решение задачи существует или нет, */
        Solution solution = Solution.Exists;
        /* Информация о решении, */
        public string information = " ";
        /* Информация о ходе решения,  */
        public List<string> information1 = new List<string>();
        /* Список шагов решения, */
        public List<SimplexTable> steps = new List<SimplexTable>();
        
         /*  Конструктор  на  основе  задачи,  */
        public SimplexMethod1(Task a1, Zadacha f1)
        {
            SimplexTable table;
            a = new List<List<double>>();
            b = new List<double>();
            c = new List<double>();
            m = a1.b.Count;
            n = a1.c.Count;
            n1 = n + m;
            List<double> u = new List<double>();
            for (int i = 0; i < m; i++)
            {
                u = new List<double>();
                b.Add(a1.b[i]);
                for (int j = 0; j < n; j++)
                {
                    u.Add(a1.a[i][j]);
                }
                a.Add(u);
            }
            for (int i = 0; i < n; i++)
            {
                c.Add(a1.c[i]);
            }
            z = 0;
            f = f1;
            for (int i = 0; i < m; i++)
            {
                c.Add(0);
                Cb.Add(0);
                basis.Add(n + i);
                for (int j = 0; j < m; j++)
                {
                    a[i].Add((i == j) ? 1 : 0);
                }                    
            }
            for (int i = 0; i < n1; i++)
            {
                x.Add(0);
                delta.Add(-1*c[i]);
                name.Add(string.Format("x{0}", i + 1));
            }
            table = new SimplexTable(
                           a, b, Cb, name, basis, delta, z);
            steps.Add(table);
            CreateSolution();
        }


        /*  Вывод решения  на форму, */
        public void OutSolution(DataGridView a1,TextBox b1)
        {
            a1.Rows.Clear();
            a1.Columns.Clear();
            b1.Text = information;
            if (solution == Solution.Exists)
            {
                a1.RowCount = 1;
                a1.ColumnCount = n1;
                a1.Rows[0].Height = 40;
                for (int i = 0; i < n1; i++)
                {
                    a1[i, 0].Value = x[i];
                    a1.Columns[i].Width = 58;
                    a1.Columns[i].HeaderCell.Value = name[i];
                }
            }
        }

        /* Решение задачи, */
        private void CreateSolution()
        {
            int i1 = 0;
            int j1 = 0;
            int k = 0;
            SimplexTable table;
            while (!IsOptimal() && IsExists() == Solution.Exists)
            {
                j1 = ResolvingColumn();
                i1 = ResolvingRow(j1);
                k = steps.Count;
                steps[k-1].ResolvingRowColumn(i1,j1);
                NewSimplexTable(i1, j1);
                information1.Add(string.Format(
                    "Из базиса выводится переменная x{0} и "+
                    "в базис вводится переменная x{1}, ",
                      basis[i1]+1,j1+1));
                basis[i1] = j1;
                Cb[i1] = c[j1];
                
                
                table = new SimplexTable(
                              a, b, Cb, name, basis, delta, z);
                steps.Add(table);
            }
            solution = IsExists();
            if (solution == Solution.Exists)
            {
                information = string.Format(
                    "Z{0}={1}, \r\nколичество шагов решения {2}, ",
                  (f == Zadacha.Max) ? "max" : "min",
                  z, steps.Count);
                information1.Add("Получено оптимальное решение,");
                for (int i = 0; i < m; i++)
                {
                    x[basis[i]] = b[i];
                }
            }
            else
            {
                information = "Задача не имеет решения, так как " +
                 " функция не ограничена на многограннике решений, ";
                information1.Add(
                       "В столбце, который может быть разрешающим, "+
                   " все элементы  неположительны, "+
                   " то есть отрицательны или равны 0, "+
                   "задача не имеет решения,");
            }
        }

        /*  Переход  к новой  симплекс-таблице,  */
        private void NewSimplexTable(int i1,int j1)
        {
            z = z * a[i1][j1] - b[i1] * delta[j1];
            z = z / a[i1][j1];
            for (int i = 0; i < m; i++)
            {
                if (i != i1)
                {
                    b[i] = b[i] * a[i1][j1] -
                                b[i1] * a[i][j1];
                    b[i] = b[i] / a[i1][j1];
                }
            }
            b[i1] = b[i1] / a[i1][j1];
            for (int j = 0; j < n1;j++)
            {
                if (j != j1)
                {
                    delta[j] = delta[j] * a[i1][j1] -
                                delta[j1] * a[i1][j];
                    delta[j] = delta[j] / a[i1][j1];
                }
            }
            delta[j1] = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n1; j++)
                {
                    if (i != i1 && j != j1)
                    {
                        a[i][j] = a[i][j] * a[i1][j1] -
                                a[i1][j] * a[i][j1];
                        a[i][j] = a[i][j] / a[i1][j1];
                    }
                }
            }
            for (int j = 0; j < n1; j++)
            {
                if (j != j1)
                {
                    a[i1][j] = a[i1][j] / a[i1][j1];
                }
            }
            a[i1][j1] = 1;
            for (int i = 0; i < m; i++)
            {
                if (i != i1)
                {
                    a[i][j1] = 0;
                }
            }
        }

        /*  Существует ли решение, */
        private Solution IsExists()
        {
            int u1 = 1;
            bool b1 = true;
            List<int> w = new List<int>();
            for (int i = 0; i < n1; i++)
            {
                if (f==Zadacha.Max && delta[i] < 0 || 
                     f==Zadacha.Min && delta[i]>0  )
                {
                    w.Add(i);
                }
            }
            for (int i = 0; i < w.Count; i++)
            {
                b1 = false;
                for (int j = 0; j < m; j++)
                {
                    if (a[j][w[i]] > 0)
                    {
                        b1 = true;
                        break;
                    }
                }
                if (b1 == false)
                {
                    u1 = 0;
                    break;
                }
            }
            return (Solution)u1;
        }

        /* Является  ли  решение  оптимальным, */
        private bool IsOptimal()
        {
            bool b1 = true;
            for (int i = 0; i < n1; i++)
            {
                if (f==Zadacha.Max && delta[i] < 0 ||
                     f==Zadacha.Min && delta[i] >0)
                {
                    b1 = false;
                    break;
                }
            }
            return b1;
        }

        /* Функция находит  разрешающий  столбец, */
        private int ResolvingColumn()
        {
            int j1 = 0;
            if (f == Zadacha.Max)
            {
                for (int i = 0; i < n1; i++)
                {
                    if (delta[i] < 0 && delta[i] < delta[j1])
                    {
                        j1 = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < n1; i++)
                {
                    if (delta[i] > 0 && delta[i] > delta[j1])
                    {
                        j1 = i;
                    }
                }
            }
            return j1;
        }

        /* Функция находит  разрешающую строку, */
        private int ResolvingRow(int j1)
        {
            int i1 = 0;
            double m1;
            double q;
            for (int i = 0; i < m; i++)
            {
                if (a[i][j1] > 0)
                {
                    i1 = i;
                    break;
                }
            }
            m1 = b[i1] / a[i1][j1];
            for(int i=0;i<m;i++)
            {
                q = b[i] / a[i][j1];
                if (a[i][j1]>0 && q<m1)
                {
                    i1 = i;
                    m1 = q;
                }
            }
            return i1;
        }

    }
}
