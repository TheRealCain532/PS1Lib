using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PS1Lib
{
    class ProcessList
    {
        public ProcessList() { }
        public bool isAttached { get { return TMAPI.Parameters.ProcessID > 0; } }
        private bool AttachProcess(ulong process)
        {
            bool isOK = false;
            PS3TMAPI.GetProcessList(TMAPI.Target, out TMAPI.Parameters.processIDs);
            if (TMAPI.Parameters.processIDs.Length > 0)
                isOK = true;
            else isOK = false;
            if (isOK)
            {
                ulong uProcess = process;
                TMAPI.Parameters.ProcessID = Convert.ToUInt32(uProcess);
                PS3TMAPI.ProcessAttach(TMAPI.Target, PS3TMAPI.UnitType.PPU, TMAPI.Parameters.ProcessID);
                PS3TMAPI.ProcessContinue(TMAPI.Target, TMAPI.Parameters.ProcessID);
                TMAPI.Parameters.info = $"The Process 0x{TMAPI.Parameters.ProcessID:X8} Has Been Attached !";
            }
            return isOK;
        }
        private string[] ProcessNames
        {
            get
            {
                PS3TMAPI.ProcessInfo current;
                string[] res = new string[TMAPI.Parameters.processIDs.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    PS3TMAPI.GetProcessInfo(0, TMAPI.Parameters.processIDs[i], out current);
                    res[i] = current.Hdr.ELFPath == null ? $"0x{TMAPI.Parameters.processIDs[i]:X} | NULL" : $"0x{TMAPI.Parameters.processIDs[i]:X} | {current.Hdr.ELFPath.Split('/').Last()}";
                }

                return res;
            }
        }
        public bool AttachProcess(string Name)
        {
            bool isOK = false;
            PS3TMAPI.GetProcessList(TMAPI.Target, out TMAPI.Parameters.processIDs);
            if (TMAPI.Parameters.processIDs.Length > 0)
                isOK = true;
            else isOK = false;
            if (isOK)
            {
                int a = 0;
                for (int i = 0; i < ProcessNames.Length; i++)
                {
                    if (ProcessNames[i].Contains(Name))
                    {
                        return AttachProcess(TMAPI.Parameters.processIDs[i]);
                    }
                }
            }
            return isOK;
        }
        public string CurrentProcessName
        {
            get
            {
                PS3TMAPI.ProcessInfo current; PS3TMAPI.GetProcessInfo(TMAPI.Target, TMAPI.Parameters.ProcessID, out current);
                return current.Hdr.ELFPath.Split('/').Last();
            }
        }
        public bool Show()
        {
            bool Result = false;
            uint SelectedProcess = 0;
            PS3TMAPI.GetProcessList(0, out TMAPI.Parameters.processIDs);
            //foreach (var item in TMAPI.Parameters.processIDs) Console.WriteLine(item.ToString("X"));
            // Instance of widgets
            Label lblInfo = new Label();
            Button btnConnect = new Button();
            Button btnRefresh = new Button();
            ListViewGroup listViewGroup = new ListViewGroup("Processes", HorizontalAlignment.Left);
            ListView listView = new ListView();
            Form formList = new Form();

            // Create our button connect
            btnConnect.Location = new Point(12, 254);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(198, 23);
            btnConnect.TabIndex = 1;
            btnConnect.Text = "Attach";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Enabled = false;
            btnConnect.Click += (sender, e) =>
            {
                if (SelectedProcess > 0)
                    if (AttachProcess(SelectedProcess))
                    {
                        formList.Close();
                        Result = true;
                    }
                    else MessageBox.Show("Failed somehow");
                else
                    MessageBox.Show("No Process Selected!");
            };

            // Create our button refresh
            btnRefresh.Location = new Point(216, 254);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(198, 23);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += (sender, e) =>
            {
                listView.Clear();
                foreach (var item in ProcessNames) listView.Items.Add(item);
            };

            // Create our list view
            listView.Font = new Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            listViewGroup.Header = "Consoles";
            listViewGroup.Name = "consoleGroup";
            listView.Groups.AddRange(new ListViewGroup[] { listViewGroup });
            listView.HideSelection = false;
            listView.Location = new Point(12, 12);
            listView.MultiSelect = false;
            listView.Name = "ConsoleList";
            listView.ShowGroups = false;
            listView.Size = new Size(400, 215);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.List;
            listView.ItemSelectionChanged += (sender, e) =>
            {
                SelectedProcess = Convert.ToUInt32(ProcessNames[e.ItemIndex].Split('|')[0].Trim(), 16);
                btnConnect.Enabled = true;
                lblInfo.Text = $"\"{ProcessNames[e.ItemIndex].Split('/').Last().Replace("\n", "")}\" Selected";
                //print pData
            };

            // Create our label
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(12, 234);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(158, 13);
            lblInfo.TabIndex = 3;
            lblInfo.Text = "Select a Process from this list!";

            // Create our form
            formList.MinimizeBox = false;
            formList.MaximizeBox = false;
            formList.ClientSize = new Size(424, 285);
            formList.AutoScaleDimensions = new SizeF(6F, 13F);
            formList.AutoScaleMode = AutoScaleMode.Font;
            formList.FormBorderStyle = FormBorderStyle.FixedSingle;
            formList.StartPosition = FormStartPosition.CenterScreen;
            formList.Text = "Select Process";
            formList.Controls.Add(listView);
            formList.Controls.Add(lblInfo);
            formList.Controls.Add(btnConnect);
            formList.Controls.Add(btnRefresh);

            // Start to update our list
            ImageList imgL = new ImageList();
            //imgL.Images.Add(Resources.ps3);
            //listView.SmallImageList = imgL;


            int sizeData = new TMAPI().SCE.ProcessIDs().Length;
            for (int i = 0; i < sizeData; i++)
            {
                ListViewItem item = new ListViewItem($" {ProcessNames[i]} ");
                item.ImageIndex = 0;
                listView.Items.Add(item);
            }

            // If there are more than 0 targets we show the form
            // Else we inform the user to create a console.
            if (sizeData > 0)
                formList.ShowDialog();
            else
            {
                Result = false;
                formList.Close();
                //MessageBox.Show(strTraduction("noConsole"), strTraduction("noConsoleTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return Result;
        }

    }


}
