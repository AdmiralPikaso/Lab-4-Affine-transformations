using System.Windows.Forms;

namespace Lab4
{
    // PictureBox с двойной буферизацией для плавной перерисовки
    public class DoubleBufferedPictureBox : PictureBox
    {
        public DoubleBufferedPictureBox()
        {
            DoubleBuffered = true;
        }
    }

    // Общий интерфейс проекта: форма, холст и кнопка.
    // Реализация блоков 1-5 находится в Task1.cs
    public sealed partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeTask1();
            InitializeTask2();
        }
    }
}
