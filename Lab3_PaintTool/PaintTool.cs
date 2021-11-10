using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Lab3_PaintTool
{
    public partial class PaintTool : Form
    {
        private Bitmap _paintImage;
        private Graphics _paintGraphics;
        private Point _lastPoint;
        private readonly ColorDialog _colorPicker = new ColorDialog();
        private bool _isMouseDown;
        private Bitmap _workingImage;
        private Graphics _workingGraphics;
        private Button _selectedShapeButton;
        private readonly PictureSerialization _serialization;
        
        private readonly string _xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "paint.xml");
        private readonly string _datFilePath = Path.Combine(Directory.GetCurrentDirectory(), "paint.dat");

        public PaintTool()
        {
            InitializeComponent();
            
            _serialization = new PictureSerialization();
            LoadPictureBox();

            _selectedShapeButton = PenButton;
            _selectedShapeButton.BackColor = Color.Red;
        }

        private void LoadPictureBox()
        {
            var width = pictureBox1.Width;
            var height = pictureBox1.Height;
            
            if (File.Exists(_xmlFilePath))
            {
                using (var fs = new FileStream(_xmlFilePath, FileMode.OpenOrCreate))
                {
                    var xmlFormatter = new XmlSerializer(typeof(PictureSerialization));
                    var binFormatter = new BinaryFormatter();
                    var paintTool = (PictureSerialization)xmlFormatter.Deserialize(fs);
                    // var paintTool = (PictureSerialization)binFormatter.Deserialize(fs);
                      
                    paintTool.Deserialize();
                    _paintImage = paintTool.PaintImage;
                    _paintGraphics = Graphics.FromImage(_paintImage);
                }
            }
            else
            {
                _paintImage = new Bitmap(width, height);
                _paintGraphics = Graphics.FromImage(_paintImage);
                _paintGraphics.FillRectangle(Brushes.White, 0, 0, width, height);
            }
            
            pictureBox1.Image = _paintImage;

            pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
            pictureBox1.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
            pictureBox1.MouseUp += new MouseEventHandler(PictureBox1_MouseUp);
        }
        
        private void PenButtonClick(object sender, EventArgs e)
        {
            _selectedShapeButton.BackColor = SystemColors.Control;

            if (!(sender is Button clickedButton)) return;
            clickedButton.BackColor = Color.Red;

            _selectedShapeButton = clickedButton;
        }
        
        private void ColorPickerButton_Click(object sender, EventArgs e)
        {
            _colorPicker.ShowDialog();
        }

        #region Drawing

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _lastPoint = e.Location;
            _isMouseDown = true;
        }
                
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            
            if (_selectedShapeButton.Text == "Pen")
                DrawLineInCanvas(e.Location);
            else
                DrawShapeInWorkingImage(e.Location);
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;

            if (_selectedShapeButton.Text == "Pen") return;
            
            DrawShapeInWorkingImage(e.Location);
            _paintImage = new Bitmap(_workingImage);
            _paintGraphics = Graphics.FromImage(_paintImage);
            pictureBox1.Image = _paintImage;
        }

        private void DrawLineInCanvas(Point currentPoint)
        {
            var pen = new Pen(_colorPicker.Color, trackBar1.Value);
            _paintGraphics.DrawLine(pen, _lastPoint, currentPoint);
            _lastPoint = currentPoint;
            pictureBox1.Refresh();
        }

        private void DrawShapeInWorkingImage(Point currentPoint)
        {
            var pen = new Pen(_colorPicker.Color, trackBar1.Value);

            _workingImage = new Bitmap(_paintImage);
            _workingGraphics = Graphics.FromImage(_workingImage);

            var startPointX = _lastPoint.X < currentPoint.X ? _lastPoint.X : currentPoint.X;
            var startPointY = _lastPoint.Y < currentPoint.Y ? _lastPoint.Y : currentPoint.Y;

            var shapeWidth = (_lastPoint.X > currentPoint.X ? _lastPoint.X : currentPoint.X) - startPointX;
            var shapeHeight = (_lastPoint.Y > currentPoint.Y ? _lastPoint.Y : currentPoint.Y) - startPointY;

            switch (_selectedShapeButton.Text)
            {
                case "Rectangle":
                    if (!FillColorCheckBox.Checked)
                        _workingGraphics.DrawRectangle(pen, startPointX, startPointY, shapeWidth, shapeHeight);
                    else
                        _workingGraphics.FillRectangle(pen.Brush, startPointX, startPointY, shapeWidth, shapeHeight);
                    break;

                case "Circle":
                    if (!FillColorCheckBox.Checked)
                        _workingGraphics.DrawEllipse(pen, startPointX, startPointY, shapeWidth, shapeHeight);
                    else
                        _workingGraphics.FillEllipse(pen.Brush, startPointX, startPointY, shapeWidth, shapeHeight);
                    break;

                case "Triangle":
                    var point1 = new Point() { X = startPointX, Y = startPointY + shapeHeight };
                    var point2 = new Point() { X = startPointX + (shapeWidth / 2), Y = startPointY };
                    var point3 = new Point() { X = startPointX + shapeWidth, Y = startPointY + shapeHeight };

                    if (!FillColorCheckBox.Checked)
                        _workingGraphics.DrawPolygon(pen, new Point[] { point1, point2, point3 });
                    else
                        _workingGraphics.FillPolygon(pen.Brush, new Point[] { point1, point2, point3 });
                    break;

                case "Line":
                    _workingGraphics.DrawLine(pen, _lastPoint, currentPoint);
                    break;
            }
            
            if (_isMouseDown && _selectedShapeButton.Text != "Line") 
            {
                var outLinePen = new Pen(Color.Black);
                outLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                _workingGraphics.DrawRectangle(outLinePen, startPointX, startPointY, shapeWidth, shapeHeight);
            }

            pictureBox1.Image = _workingImage;
        }

        #endregion

        #region NewExit

        private void New(object sender, EventArgs e)
        {
            var width = pictureBox1.Width;
            var height = pictureBox1.Height;
            
            _paintImage = new Bitmap(width, height);
            _paintGraphics = Graphics.FromImage(_paintImage);
            _paintGraphics.FillRectangle(Brushes.White, 0, 0, width, height);
            pictureBox1.Image = _paintImage;
        }

        private void ExitApp(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to Exit?",
                "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
            
            SerializeToXml();
            SerializeToBinary();
            
            Application.Exit();
        }

        #endregion
        
        #region SaveLoad

        private void SaveFile(object sender, EventArgs e)
        {
            var saveFileDialog1 = new SaveFileDialog();  
            saveFileDialog1.FileName = "image";
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            saveFileDialog1.Filter = "JPG|*.jpg|BMP|*.bmp|PNG|*.png";
            saveFileDialog1.Title = "Save Image";

            if (saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if (saveFileDialog1.FileName.Length > 0)
            {
                _paintImage.Save(saveFileDialog1.FileName);
            }
        }
        private void LoadFile(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog1.Filter = "JPG|*.jpg|BMP|*.bmp|PNG|*.png|All Files|*.*";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (openFileDialog1.FileName.Length > 4)
                {
                    var width = pictureBox1.Width;
                    var height = pictureBox1.Height;
            
                    _paintImage = new Bitmap(openFileDialog1.FileName);
                    _paintGraphics = Graphics.FromImage(_paintImage);
                    pictureBox1.Image = _paintImage;
                }
            }
        }

        #endregion


        private void Form1_FormClosing(object sender, FormClosingEventArgs e) 
        {
            SerializeToXml();
            // SerializeToBinary();
        }

        private void SerializeToXml()
        {
            _serialization.PaintImage = _paintImage;
            _serialization.Serialize();
            
            var formatter = new XmlSerializer(typeof(PictureSerialization));
            using (var fs = new FileStream(_xmlFilePath, FileMode.OpenOrCreate))
                formatter.Serialize(fs, _serialization);
        }

        private void SerializeToBinary()
        {
            _serialization.PaintImage = _paintImage;
            _serialization.Serialize();
            
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(_datFilePath, FileMode.OpenOrCreate))
                formatter.Serialize(fs, _serialization);
        }
    }
}