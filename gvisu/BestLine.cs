using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using GomokuEngine;

namespace gvisu
{
    public partial class BestLine : Form
    {
        public BestLine()
        {
            InitializeComponent();
			this.Resize += new EventHandler(BestLine_Resize);
        }

		void BestLine_Resize(object sender, EventArgs e)
		{
			listView1.Height = this.Height;
			listView1.Width = this.Width;
		}

        public void RefreshBestLine(List<ABMove> bestLine)
        {
            //display possible moves
            listView1.Items.Clear();

            if (bestLine != null)
            {
                List<ABMove> bl = new List<ABMove>(bestLine);

                for (int index = 0; index < bl.Count; index++)
                {

                    string[] str = bl[index].ToStringArray(index+1);
                    ListViewItem listViewItem = new ListViewItem(str);
                    listViewItem.Tag = bl[index].ToString();
                    listView1.Items.Add(listViewItem);
                }
            }
        }

		private void BestLine_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Hide();
			e.Cancel = true;
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
    }
}