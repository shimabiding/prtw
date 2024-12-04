using System;
using System.Windows.Forms;
using System.Drawing;

namespace wp
{
    public partial class wrapperForm : Form
    {
        TextBox textBox1;
        Button button1;

        public wrapperForm()
        {
            InitializeComponent();
        }

        void InitializeComponent()
        {
            this.Text = "一般印刷ラッパー";
            this.ClientSize = new System.Drawing.Size(500, 200);

            this.Controls.Add(textBox1 = new TextBox
            {
                Location = new Point(5, 0),
                Size = new Size(200, 30)
            });

            this.Controls.Add(button1 = new Button
            {
                Location = new Point(5, 30),
                Size = new Size(50, 30),
                Text = "Open msgbox"
            });

            button1.Click += Button1_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var s = Program.GetValidatedInput(textBox1.Text);
            MessageBox.Show(s);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            var text = Program.GetValidatedInput(textBox1.Text);
            if (null == text) return;


            using (var conn = Program.CreateDatabaseConnection())
            {
                if (null == conn) return;
                var result = Program.ExecuteQueryAndGetResult(conn, text);
                if (null != result)
                {
                    Program.UpdateUserInterface(result);
                }
                else
                {
                    MessageBox.Show("入力されたIDは存在しません。");
                }

                textBox1.Text = "";
            }
        }
    }
}