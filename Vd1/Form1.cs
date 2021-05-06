using Alturos.Yolo;
using Alturos.Yolo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vd1
{
    public partial class Form1 : Form
    {
        YoloWrapper yoloWrapper;
        List<string> listImages;
        string _fileName;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            /*
             Chọn tất cả các ảnh, tên ảnh thì đưa và datagridview, còn ảnh thì đưa
            ra picturebox. Mục tiêu hiện giờ là làm sao click chọn vào từng dòng
            trên datagridview (kiểu datagridview.selecteditem) thì picturebox 
            sẽ nhảy đến ảnh đấy
             */
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();  //đầu tiên sẽ hiển thị một hộp dialog, mở đến đâu sẽ cập nhật hộp dialog đấy là fbd
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))  //nếu chọn được một dialog fbd bằng cách ấn OK thì
                {
                    var files = Directory.GetFiles(fbd.SelectedPath);   // thực ra có thể dùng luôn directory.GetFiles(fbd.SelectedPath) để đưa vào vòng lặp foreach nhưng tác giả đéo thích gõ nhiều nên gán vào var files cho nhanh
                    var table = new DataTable();  //tạo bảng để sau nhét vào datagridview
                    table.Columns.Add("File name"); //tạo một column tên là File name
                    foreach (var file in files)
                    {

                        if (Regex.IsMatch(file, @".jpg|.jpeg|.png|.gif|.bmp|.JPG|.PNG|.JPEG|.BMP$"))  //lọc từng tên file một có trong files nếu có đuôi như kia thì 
                        {
                            listImages.Add(file);  //1. đưa vào listImage để sau xuất ảnh vào pictureBox
                            table.Rows.Add(Path.GetFileNameWithoutExtension(file));  //2.đưa tên file vào collumn file name của table nhưng lần này là đưa theo hàng, và lược bỏ địa chỉ của file bằng Path.GetFileNameWithoutExtension 
                        }
                    }
                    dataGridView1.DataSource = table;  //sau khi đã tạo được table hoàn chỉnh rồi thì nhét nó vào table
                }
            }
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            /*
             đúng ra theo lý thuyết thì nó sẽ là thế này, tuy nhiên nếu muốn detect
            nhiều ảnh thì phải tìm cách truyền được nhiều string khác nhau vào 
            yolowrapper.detect, chính là cái _fileName tạo lúc load form1 đấy
             */
            var items = yoloWrapper.Detect(_fileName);  //cái items này chính là vật mà yolo đã detect ra trong mỗi ảnh
            //var items = yoloWrapper.Detect(listImages[dataGridView1.CurrentRow.Index]);
            //detect xong rồi thì phải tạo hàm để vẽ đường bao cho nó chuyên nghiệp
            drawBorderResult(items.ToList(), _fileName);
        }

        private void drawBorderResult(List<YoloItem> yoloItems, string fileName)
        {
            var image = Image.FromFile(fileName);  //tạo biến lưu file ảnh
            Random rnd = new Random();
            using (var canvas = Graphics.FromImage(image))
            {
                foreach (var item in yoloItems)
                {
                    var x = item.X;
                    var y = item.Y;
                    var width = item.Width;
                    var height = item.Height;
                    var confidence = item.Confidence.ToString("P",CultureInfo.InvariantCulture);
                    
                    Color color = Color.FromArgb(200, rnd.Next(100, 200), rnd.Next(100, 200));
                    using (var pen = new Pen(color, 3))
                    {
                        canvas.DrawRectangle(pen, x, y, width, height);
                        canvas.DrawString(item.Type + " " + confidence, new Font("Arial", 14), new SolidBrush(color), x, y);
                        canvas.Flush();   //push dữ liệu đó lên ảnh, buộc thực hiện tất cả các hoạt động đồ họa đang chờ xử lý lên anh mà không cần đợi chúng chạy xong
                    }

                }
                pictureBox1.Image = image;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Startup();
        }
        void Startup()
        {
            listImages = new List<string>();
            var configurationDetector = new YoloConfigurationDetector();
            var config = configurationDetector.Detect();
            yoloWrapper = new YoloWrapper(config);  //phải nhớ là cái này được tạo từ đầu rồi, không tạo mới nữa nếu không nó sẽ null

        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            /*
             Tạo một list để lưu các đường dẫn của mỗi ảnh khi chọn
            ở đây chọn là fileName
             */
            _fileName = listImages[dataGridView1.CurrentRow.Index];

            pictureBox1.Image = new Bitmap(_fileName);


        }
    }
}