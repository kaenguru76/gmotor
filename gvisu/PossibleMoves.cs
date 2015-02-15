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
    public partial class PossibleMoves : Form
    {
        List<ABMove> possibleMoves;
		Engine engine;
		ABMove selectedMove;

		public PossibleMoves(Engine engine)
        {
            InitializeComponent();
			this.Resize += new EventHandler(PossibleMoves_Resize);
			this.engine = engine;
            engine.MovesChangedE += new Engine.MovesChangedEvent(engine_PossibleMovesChanged);
            engine.ThinkingProgress += new Engine.ThinkingProgressEvent(engine_ThinkingProgress);
            engine.ThinkingFinished += new Engine.ThinkingFinishedEvent(engine_ThinkingFinished);
        }

        void PossibleMoves_Resize(object sender, EventArgs e)
		{
			listView1.Height = this.Height;
			listView1.Width = this.Width;
		}

        void engine_PossibleMovesChanged(GameInformation gameInformation)
        {
            //display possible moves
            listView1.Items.Clear();
            this.possibleMoves = gameInformation.possibleMoves;
            for (int index = 0; index < possibleMoves.Count; index++)
            {

                string[] str = possibleMoves[index].ToStringArray(index + 1);
                ListViewItem listViewItem = new ListViewItem(str);
                listViewItem.Tag = possibleMoves[index].ToString();
                listView1.Items.Add(listViewItem);
            }
            this.Text = listView1.Items.Count + " possible move(s)";
            checkBoxThreat.Checked = engine.VctActive;
        }

        delegate void ThinkingProgressDelegate(SearchInformation info);

        void engine_ThinkingProgress(SearchInformation info)
        {
            if (this.IsHandleCreated)
            {
                ThinkingProgressDelegate delegate1 = new ThinkingProgressDelegate(ThinkingProgress);
                this.BeginInvoke(delegate1, info);
            }
        }

        delegate void ThinkingFinishedDelegate(SearchInformation info);

        void engine_ThinkingFinished(SearchInformation info)
        {
            if (this.IsHandleCreated)
            {
                ThinkingFinishedDelegate delegate1 = new ThinkingFinishedDelegate(ThinkingFinished);
                this.BeginInvoke(delegate1, info);
            }
        }

        void ThinkingProgress(SearchInformation info)
        {
			//display possible moves
            listView1.Items.Clear();
            if (info.possibleMoves != null)
            {
                for (int index = 0; index < info.possibleMoves.Count; index++)
                {

                    string[] str = info.possibleMoves[index].ToStringArray(index + 1);
                    ListViewItem listViewItem = new ListViewItem(str);
                    listViewItem.Tag = info.possibleMoves[index].ToString();
                    listView1.Items.Add(listViewItem);
                }
            }

            this.Text = listView1.Items.Count + " possible move(s)";
            checkBoxThreat.Checked = info.vctActive;
        }

        void ThinkingFinished(SearchInformation info)
        {
            ThinkingProgress(info);
        }
            
        private void PossibleMoves_Load(object sender, EventArgs e)
		{
            //engine.Redraw();
		}

		private void buttonPlayMove_Click(object sender, EventArgs e)
		{
			engine.MakeMove(selectedMove);
		}

		private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
            selectedMove = possibleMoves[e.ItemIndex];
		}

        private void checkBoxThreat_Click(object sender, EventArgs e)
        {
            engine.VctActive = checkBoxThreat.Checked;
        }

        private void checkBoxThreat_CheckedChanged(object sender, EventArgs e)
        {

        }

    }
}