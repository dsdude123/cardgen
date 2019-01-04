using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cardgenFunction.Models;
using Newtonsoft.Json;

namespace cardgenDesktopInterface
{
    public partial class Form1 : Form
    {
        private Image hearthstone = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            hearthstone = Image.FromStream(openFileDialog1.OpenFile());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HearthstoneCard card = new HearthstoneCard();
            card.cardName = textBox2.Text;
            card.cardType = (HearthstoneCard.CardType)comboBox1.SelectedIndex;
            card.cardClass = (HearthstoneCard.CardClass)comboBox2.SelectedIndex;
            card.text = textBox6.Text;
            card.cost = textBox3.Text;
            card.attack = textBox4.Text;
            card.health = textBox5.Text;
            Stream arttobyte = new MemoryStream();
            hearthstone.Save(arttobyte,ImageFormat.Png);
            byte[] beforebase64 = new byte[arttobyte.Length];
            arttobyte.Seek(0, SeekOrigin.Begin);
            arttobyte.Read(beforebase64, 0, (int)arttobyte.Length);
            card.artwork = Convert.ToBase64String(beforebase64);
            card.rarity = (HearthstoneCard.CardRarity) comboBox3.SelectedIndex;
            CardGenerationRequest req = new CardGenerationRequest();
            req.hearthstoneCard = card;
            req.type = CardGenerationRequest.CardType.Hearthstone;
            HttpClient http = new HttpClient();
            HttpResponseMessage msg = http.PostAsync(textBox1.Text, new StringContent(JsonConvert.SerializeObject(req))).Result;
            msg.EnsureSuccessStatusCode();
            CardGenerationResponse rep =
                JsonConvert.DeserializeObject<CardGenerationResponse>(msg.Content.ReadAsStringAsync().Result);
            byte[] frombase64 = Convert.FromBase64String(rep.card);
            Image outputImage = new Bitmap(new MemoryStream(frombase64));
            outputImage.Save("output.png");
        }
    }
}
