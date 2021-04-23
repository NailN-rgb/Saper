using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Miner.Controllers
{
    public static class MapController
    {
        public const int mapSize = 20;
        public const int cellSize = 30;
        public const int bombNumber = 10;
        private static int currentPictureToSet = 0;

        public static int[,] map = new int[mapSize, mapSize];
        public static int check = 0;

       

        public static Button[,] buttons = new Button[mapSize, mapSize];

        public static Image spriteSet;

        private static bool isFirstStep;

        private static Point firstCoord;

        public static Form form;

        private static void Form1_Load(object sender, EventArgs e)
        {
            //я умру скоро
        }

        private static void ConfigureMapSize(Form current)
        {
            current.Width = mapSize * cellSize + 20;
            current.Height = (mapSize + 1) * cellSize + 15;
        }

        //заполнение массива map

        private static void ToMap()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = 0;
                    
                }
            }
        }

        public static void Init(Form current)
        {
            form = current;
            currentPictureToSet = 0;
            isFirstStep = true;
            spriteSet = new Bitmap(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName.ToString(), "Sprites\\текстуры.png"));
            ConfigureMapSize(current);
            ToMap();
            ToButtons(current);
            Winning();
            current.MaximizeBox = false;
            current.MinimizeBox = false;
        }

        //динамическое создание полей,
        private static void ToButtons(Form current)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Image = Image(0, 0);
                    button.MouseUp += new MouseEventHandler(OnButtonPressedMouse);
                    current.Controls.Add(button);
                    buttons[i, j] = button;
                }
            }
        }

        //функция события обработки события нажатия кнопки мыши
        private static void OnButtonPressedMouse(object sender, MouseEventArgs e)
        {
            
            Button pressedButton = sender as Button;
            switch (e.Button.ToString())
            {
                case "Right":
                    OnRightButtonPressed(pressedButton);
                    break;
                case "Left":
                    OnLeftButtonPressed(pressedButton);
                    break;
            }
        }

        //функция нажатия правой кнокой мыши
        private static void OnRightButtonPressed(Button pressedButton)
        {
            currentPictureToSet++;
            currentPictureToSet %= 2;
            int posX = 0;
            int posY = 0;
            switch (currentPictureToSet)
            {
                case 0:
                    posX = 0;
                    posY = 0;
                    break;
                case 1:
                    posX = 10;
                    posY = 0;
                    break;
               
            }
            pressedButton.Image = Image(posX, posY);
            check++;
        }

        //функция обработки нажатия левой кнокой мыши
        private static void OnLeftButtonPressed(Button pressedButton)
        {
            pressedButton.Enabled = false;
            int iButton = pressedButton.Location.Y / cellSize;
            int jButton = pressedButton.Location.X / cellSize;

            //если выбор поля произведен впервые
            if (isFirstStep)
            {
                firstCoord = new Point(jButton, iButton);
                //заполняем карту
                FillMap();
                //заполнение клеток бомбами, первое нажатое поле будет гарантированно свободно от бомб
                BombCounter();
                isFirstStep = false;
            }
            OpenCells(iButton, jButton);

            //если выбранное поле - мина
            if (map[iButton, jButton] == -1)
            {
                ShowAllBombs(iButton, jButton);
                MessageBox.Show("Поражение!");
                form.Controls.Clear();
                //рекурсивный запуск игры заноов
                Init(form);
            }
        }


        public static void Winning()
        {
            if (check == bombNumber - 1)
            {
                MessageBox.Show("Вы победили", "Ок", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }
        //открытие карты после поражения
        private static void ShowAllBombs(int iBomb, int jBomb)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (i == iBomb && j == jBomb)
                        continue;
                    if (map[i, j] == -1)
                    {
                        buttons[i, j].Image = Image(12, 0);
                    }
                }
            }
        }

        //инициализация текстур
        public static Image Image(int xPos, int yPos)
        {
            Image image = new Bitmap(cellSize, cellSize);
            Graphics g = Graphics.FromImage(image);

            //извлечение и вырезание нужной текстуры из заранее подготовленного изображения
            g.DrawImage(spriteSet, new Rectangle(new Point(0, 0), new Size(cellSize, cellSize)), 0 + 17 * xPos, 0 * yPos, 17, 17, GraphicsUnit.Pixel);


            return image;
        }

        //заполнение поля минами
        public static void FillMap()
        {
            Random r = new Random();
            int number = bombNumber;

            for (int i = 0; i < number; i++)
            {
                int posI = r.Next(0, mapSize - 1);
                int posJ = r.Next(0, mapSize - 1);

                while (map[posI, posJ] == -1 || (Math.Abs(posI - firstCoord.Y) <= 1 && Math.Abs(posJ - firstCoord.X) <= 1))
                {
                    posI = r.Next(0, mapSize - 1);
                    posJ = r.Next(0, mapSize - 1);
                }
                map[posI, posJ] = -1;
            }
        }

        //расчет значений количества бомб вокруг указанной клетки, присваивание в массив Map
        private static void BombCounter()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == -1)
                    {
                        for (int k = i - 1; k < i + 2; k++)
                        {
                            for (int l = j - 1; l < j + 2; l++)
                            {
                                if (!InBorder(k, l) || map[k, l] == -1)
                                {
                                    continue;
                                }
                                map[k, l] = map[k, l] + 1;
                            }
                        }
                    }
                }
            }
        }

        //открытие поля, если вокруг клетки мин нет - открытие бОльшего количества клеток
        private static void OpenCell(int i, int j)
        {
            buttons[i, j].Enabled = false;

            switch (map[i, j])
            {
                //испольщование нужного куска текстуры в зависимости от значения map[]
                case 1:
                    buttons[i, j].Image = Image(1, 0);
                    break;
                case 2:
                    buttons[i, j].Image = Image(2, 0);
                    break;
                case 3:
                    buttons[i, j].Image = Image(3, 0);
                    break;
                case 4:
                    buttons[i, j].Image = Image(4, 0);
                    break;
                case 5:
                    buttons[i, j].Image = Image(5, 0);
                    break;
                case 6:
                    buttons[i, j].Image = Image(6, 0);
                    break;
                case 7:
                    buttons[i, j].Image = Image(7, 0);
                    break;
                case 8:
                    buttons[i, j].Image = Image(8, 0);
                    break;
                case -1:
                    buttons[i, j].Image = Image(12, 0);
                    break;
                case 0:
                    buttons[i, j].Image = Image(13, 0);
                    break;
            }
        }

        private static void OpenCells(int i, int j)
        {
            OpenCell(i, j);

            if (map[i, j] > 0)
                return;

            for (int k = i - 1; k < i + 2; k++)
            {
                for (int l = j - 1; l < j + 2; l++)
                {
                    if (!InBorder(k, l))
                    {
                        continue;
                    }
                    if (!buttons[k, l].Enabled)
                    {
                        continue;
                    }
                    if (map[k, l] == 0)
                    {
                        OpenCells(k, l);
                    }
                    else if (map[k, l] > 0)
                    {
                        OpenCell(k, l);
                    }
                }
            }
        }

        //проверка на границы массива
        private static bool InBorder(int i, int j)
        {
            if (i < 0 || j < 0 || j > mapSize - 1 || i > mapSize - 1)
            {
                return false;
            }
            return true;
        }
    }
}