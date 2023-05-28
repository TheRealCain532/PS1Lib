using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS1Lib
{
   public class OpenGamesharkFile
    {

        String[] cheats = new string[10000];
        string gFilename, gFile;
        int g = 0;
        public OpenGamesharkFile()
        { 
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Cheat File|*.ini";
            if (of.ShowDialog() == DialogResult.OK)
            {
                gFilename = of.SafeFileName;
                gFile = File.ReadAllText(of.FileName);
                this.Show();
            }
        }
        bool Show()
        {
            bool result = false;
            String[] cheatlist = Regex.Replace(gFile, @"^\s+$[\r\n]*", "", RegexOptions.Multiline).Split('\n');
            Panel panel = new Panel();
            Form sharkForm = new Form();
            CheckBox[] box = new CheckBox[cheatlist.Length];
            //g = 0;
            try
            {
                for (int i = 0; i < cheatlist.Length; i++)
                {
                    if (!cheatlist[i].StartsWith(".") & g > 0)
                        cheats[g - 1] += cheatlist[i];
                    if (cheatlist[i].StartsWith("\""))
                    {
                        box[i] = new CheckBox();
                        box[i].Name = g.ToString();
                        box[i].Text = cheatlist[i].Replace("\"", "");
                        box[i].AutoSize = true;
                        box[i].Location = new Point(10, g * 20); //vertical
                        panel.Controls.Add(box[i]);
                        g++;
                        box[i].CheckedChanged += (sender, e) =>
                        {
                            CheckBox b = (CheckBox)sender;
                            int index = Convert.ToInt32(b.Name);
                            string[] codes = cheats[index].Split('\r');
                            new GameShark().Write(codes);
                        };
                    }
                }
            }
            catch { }

            panel.AutoScroll = true;
            panel.Dock = DockStyle.Fill;
            panel.Name = "panel1";
            sharkForm.MinimizeBox = true;
            sharkForm.MaximizeBox = false;
            sharkForm.ClientSize = new Size(285, 230);
            sharkForm.AutoScaleDimensions = new SizeF(6F, 13F);
            sharkForm.AutoScaleMode = AutoScaleMode.Font;
            sharkForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            sharkForm.StartPosition = FormStartPosition.CenterScreen;
            sharkForm.Text = gFilename;
            sharkForm.Controls.Add(panel);
            sharkForm.ShowDialog();
            return result;
        }
    }
    }
