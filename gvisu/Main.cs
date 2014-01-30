﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.Reflection;

using GomokuEngine;

namespace gvisu
{
	public partial class Main : Form
	{
		Engine engine;
		ImageList BoardImageList;
        ImageList BoardImageListWithPoint;
        ImageList SemaforImageList;
        ImageList ToolbarImageList;
        PictureBox[,] picSquares;
        Image selectedImage;
        PictureBox selectedPictureBox;

        Conversions conversions;
        PossibleMoves possibleMoves;
        BestLine bestLine;
        List<ABMove> playedMoves;

        public Main()
		{
			InitializeComponent();

            engine = new Engine();
            engine.NewGameE += new Engine.NewGameEvent(engine_NewGameE);
            engine.MovesChangedE += new Engine.MovesChangedEvent(engine_MovesChanged);
            engine.ThinkingFinished += new Engine.ThinkingFinishedEvent(engine_ThinkingFinished);
            engine.ThinkingProgress += new Engine.ThinkingProgressEvent(engine_ThinkingProgress);
        }

		private void Main_Load(object sender, EventArgs e)
		{

            //load wood image list
            Bitmap bitmaps = Properties.Resources.toolbar;
            ToolbarImageList = new ImageList();
            ToolbarImageList.ImageSize = new Size(bitmaps.Size.Height, bitmaps.Size.Height);
            ToolbarImageList.Images.AddStrip(bitmaps);
            toolStrip1.ImageList = ToolbarImageList;
            newToolStripButton.ImageIndex = 0;
            openToolStripButton.ImageIndex = 1;
            saveToolStripButton.ImageIndex = 2;
            firstToolStripButton.ImageIndex = 3;
            previousToolStripButton.ImageIndex = 4;
            nextToolStripButton.ImageIndex = 5;
            lastToolStripButton.ImageIndex = 6;

            bestLine = new BestLine();
        }

		void engine_NewGameE()
        {
			PictureBox picSquare;
			Label lblRow;
			Label lblColumn;

            conversions = new Conversions(engine.BoardSize);

            //load wood image list
			Bitmap bitmaps = Properties.Resources.wood;
			BoardImageList = new ImageList();
			BoardImageList.ImageSize = new Size(bitmaps.Size.Height, bitmaps.Size.Height);
			BoardImageList.Images.AddStrip(bitmaps);

            //modify bitmaps - draw point
            Bitmap bmpWithPoint = new Bitmap(bitmaps);
            Graphics gWithPoint = Graphics.FromImage(bmpWithPoint);
            gWithPoint.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            Brush whiteBrush = new SolidBrush(Color.White);
            for (int index = 0; index < BoardImageList.Images.Count; index++)
            {
                Rectangle rect = new Rectangle(bmpWithPoint.Height/2 - 1 + index*(bmpWithPoint.Height), bmpWithPoint.Height/2 - 1, 3, 3);
                gWithPoint.FillRectangle(whiteBrush, rect);
            }

            //create wood image list with point
            BoardImageListWithPoint = new ImageList();
            BoardImageListWithPoint.ImageSize = BoardImageList.ImageSize;
            BoardImageListWithPoint.Images.AddStrip(bmpWithPoint);

            //load semafor image list
            bitmaps = Properties.Resources.semafor;
            SemaforImageList = new ImageList();
            SemaforImageList.ImageSize = new Size(bitmaps.Size.Height, bitmaps.Size.Height);
            SemaforImageList.Images.AddStrip(bitmaps);
            
            // resize board
			panelBoard.Size = new Size(BoardImageList.ImageSize.Width * (engine.BoardSize + 1), BoardImageList.ImageSize.Width * (engine.BoardSize + 1));
			panelBoard.Controls.Clear();

			// shift control panel
			panelControl.Left = panelBoard.Right;
			panelControl.Height = panelBoard.Height;

            //resize listViews
            //listViewGame.Left = panelBoard.Right;
            listViewGame.Height = panelControl.Height - listViewGame.Top;


            // resize whole form
			this.ClientSize = new Size(panelControl.Right, panelBoard.Bottom + statusStrip1.Height);


			//set back colour
			panelBoard.BackColor = bitmaps.GetPixel(0, 0);

			//draw row legend
			for (int row = 0; row < engine.BoardSize; row++)
			{
				lblRow = new Label();
				lblRow.BackColor = System.Drawing.Color.Transparent;
				lblRow.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				lblRow.Name = "lblRow" + row.ToString();
				lblRow.Size = BoardImageList.ImageSize;
				lblRow.Top = panelBoard.Height - BoardImageList.ImageSize.Height - (row + 1) * BoardImageList.ImageSize.Height;
				lblRow.Left = 0;
				lblRow.Text = conversions.Row(row);
				lblRow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				panelBoard.Controls.Add(lblRow);
			}

			//draw column legend
			for (int column = 0; column < engine.BoardSize; column++)
			{
				lblColumn = new Label();
				lblColumn.BackColor = System.Drawing.Color.Transparent;
				lblColumn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				lblColumn.Name = "lblColumn" + column.ToString();
				lblColumn.Size = BoardImageList.ImageSize;
				lblColumn.Top = panelBoard.Height - BoardImageList.ImageSize.Height;
				lblColumn.Left = (column+1) * BoardImageList.ImageSize.Width;
				lblColumn.Text = conversions.Column(column);
				lblColumn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				panelBoard.Controls.Add(lblColumn);

			}

			//define square pictures
			picSquares = new PictureBox[engine.BoardSize, engine.BoardSize];

			//draw board
			for (int column = 0; column < engine.BoardSize; column++)
			{
				for (int row = 0; row < engine.BoardSize; row++)
				{
					picSquare = new System.Windows.Forms.PictureBox();
					picSquare.Image = BoardImageList.Images[0];
					picSquare.Name = "picSquare" + conversions.Complete(row , column);
					picSquare.Size = BoardImageList.ImageSize;
					picSquare.Top = panelBoard.Height - BoardImageList.ImageSize.Height - (row + 1) * picSquare.Size.Height;
					picSquare.Left = BoardImageList.ImageSize.Width + column * picSquare.Size.Height;
					picSquare.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
					picSquare.TabStop = false;
                    picSquare.Tag = conversions.Complete(row, column); 
                    picSquare.MouseClick +=new MouseEventHandler(picSquare_MouseClick);
					panelBoard.Controls.Add(picSquare);
					picSquares[row, column] = picSquare;
				}
			}

			//draw 2 symbols
			buttonBlack.Image = BoardImageList.Images[1];
			//buttonBlack.Size = BoardImageList.ImageSize;
			buttonWhite.Image = BoardImageList.Images[2];
			//buttonWhite.Size = BoardImageList.ImageSize;

            //upgrade title
            this.Text = Application.ProductName + " - " + engine.gameInformation.fileName;
            lblBlack.Text = engine.gameInformation.blackPlayerName;
            lblWhite.Text = engine.gameInformation.whitePlayerName;
            numericUpDownThinkTime.Value = (decimal)(engine.MaxThinkingTime.TotalMilliseconds / 1000);
            numericUpDownDepth.Value = (decimal)(engine.MaxSearchDepth);
            numericUpDownDfPnHash.Value = (decimal)(engine.TranspositionTableSize / 1000000);

            mnuShowBestLine.Enabled = true;
            mnuShowPossibleMoves.Enabled = true;
            heuristicsToolStripMenuItem.Enabled = true;
        }

        void engine_MovesChanged(GameInformation gameInformation)
        {
            this.playedMoves = gameInformation.playedMoves;

            //erase board
            foreach (PictureBox pic in picSquares)
            {
                pic.Image = BoardImageList.Images[0];
            }
            listViewGame.Items.Clear();

            for (int index = 0; index < playedMoves.Count; index++)
            {
                ABMove move = playedMoves[index];

                string[] str = move.ToStringArray(index + 1);
                ListViewItem listViewItem = new ListViewItem(str);

                listViewGame.Items.Add(listViewItem);

                if (move.player == Player.BlackPlayer)
                {
                    picSquares[move.Row, move.Column].Image = BoardImageList.Images[1];
                }
                else
                {
                    picSquares[move.Row, move.Column].Image = BoardImageList.Images[2];
                }
            }
            //scroll listbox down
            if (listViewGame.Items.Count > 0)
            {
                listViewGame.TopItem = listViewGame.Items[listViewGame.Items.Count - 1];
            }

            //make point on last move
            if (playedMoves.Count > 0)
            {
                ABMove move = playedMoves[playedMoves.Count - 1];

                switch (move.player)
                {
                    case Player.None:
                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[0];
                        break;

                    case Player.BlackPlayer:
                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[1];
                        break;

                    case Player.WhitePlayer:
                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[2];
                        break;
                }
            }

            //disable next move button, if there is no next move
            if (gameInformation.nextMove != null)
            {
                nextToolStripButton.Enabled = true;
                lastToolStripButton.Enabled = true;
                nextToolStripMenuItem.Enabled = true;
                lastToolStripMenuItem.Enabled = true;
            }
            else
            {
                nextToolStripButton.Enabled = false;
                lastToolStripButton.Enabled = false;
                nextToolStripMenuItem.Enabled = false;
                lastToolStripMenuItem.Enabled = false;
            }

            //disable previous move button, if there is no previous move
            if (playedMoves.Count == 0)
            {
                firstToolStripButton.Enabled = false;
                previousToolStripButton.Enabled = false;
                firstToolStripMenuItem.Enabled = false;
                previousToolStripMenuItem.Enabled = false;
            }
            else
            {
                firstToolStripButton.Enabled = true;
                previousToolStripButton.Enabled = true;
                firstToolStripMenuItem.Enabled = true;
                previousToolStripMenuItem.Enabled = true;
            }

            //point on next move 
            if (gameInformation.nextMove != null)
            {
                picSquares[gameInformation.nextMove.Row, gameInformation.nextMove.Column].Image = BoardImageListWithPoint.Images[0];
            }
            
            //refresh possible moves
            for (int index = 0; index < gameInformation.possibleMoves.Count; index++)
			{
                ABMove move = gameInformation.possibleMoves[index];

				if (move.player == Player.BlackPlayer)
				{
                    if (gameInformation.nextMove != null && move.square == gameInformation.nextMove.square)
                    {
                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[3];
                    }
                    else
					{
						picSquares[move.Row, move.Column].Image = BoardImageList.Images[3];
					}
				}
				else
				{
                    if (gameInformation.nextMove != null && move.square == gameInformation.nextMove.square)
                    {
                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[4];
                    }
                    else
                    {
						picSquares[move.Row, move.Column].Image = BoardImageList.Images[4];
					}
				}
			}

            statusStrip1.Items[1].Text = "Evaluation = " + gameInformation.EvaluationTotal.ToString() + " (black:" +
                gameInformation.BlackScore + ", white:" + gameInformation.WhiteScore + ")";
            statusStrip1.Items[1].Visible = true;

        }

		private void picSquare_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SelectedPictureBox = (PictureBox)sender;
        }

        private void New()
		{
            //open window with parameters for new game
			NewGame newGame = new NewGame();
			if (newGame.ShowDialog() == DialogResult.OK)
			{
                //say to engine, that it is new game
                engine.NewGame(newGame.BoardSize);
                //enable items
                mnuSave.Enabled = true;
                mnuSaveAs.Enabled = true;
                mnuReopen.Enabled = true;
                saveToolStripButton.Enabled = true;
            }
            //dispose dialog
			newGame.Close();
		}
		
		private void Open()
		{
            OpenFileDialog openFileDialog = new OpenFileDialog();
            OpenFile openFile=null;

            openFileDialog.Title = "Load game";
            openFileDialog.Filter = "All gomoku files (*.psq;*.rec)|*.psq;*.rec|Piskvorky (*.psq)|*.psq|Piskvorky (*.rec)|*.rec";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog.FileName != "")
                {
                    openFile = new OpenFile(openFileDialog.FileName);
                    if (openFile != null && openFile.fileParameters.MoveList == null) return;
                    engine.gameInformation.fileName = openFileDialog.FileName;
                    engine.gameInformation.blackPlayerName = openFile.fileParameters.BlackPlayerName;
                    engine.gameInformation.whitePlayerName = openFile.fileParameters.WhitePlayerName;
					engine.LoadGame(openFile.fileParameters.BoardSize, openFile.fileParameters.MoveList);
                    //enable items
                    mnuSave.Enabled = true;
                    mnuSaveAs.Enabled = true;
                    mnuReopen.Enabled = true;
                    saveToolStripButton.Enabled = true;
                }
            }

		}

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Save " + engine.gameInformation.fileName;
            saveFileDialog.Filter = "Piskvorky (*.psq)|*.psq|Piskvorky (*.rec)|*.rec";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.AddExtension = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ValidateNames = true;
            saveFileDialog.FileName = engine.gameInformation.fileName;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FileName != "")
                {
					SaveFile saveFile = new SaveFile(saveFileDialog.FileName, engine.BoardSize, engine.gameInformation.blackPlayerName,
                        engine.gameInformation.whitePlayerName, playedMoves);
                    //remember filename
                    if (saveFile != null)
                    {
                        engine.gameInformation.fileName = saveFileDialog.FileName;

						//upgrade title
						this.Text = Application.ProductName + " - " + engine.gameInformation.fileName;
					}
                }
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
		{
			New();
		}

		private void openToolStripButton_Click(object sender, EventArgs e)
		{
			Open();
		}

		private void firstToolStripButton_Click(object sender, EventArgs e)
		{
			engine.UndoAll();
		}

		private void previousToolStripButton_Click(object sender, EventArgs e)
		{
			engine.Undo();
		}

		private void nextToolStripButton_Click(object sender, EventArgs e)
		{
			engine.Redo();
		}

		private void lastToolStripButton_Click(object sender, EventArgs e)
		{
			engine.RedoAll();
		}

		private void mnuHumanStarts_Click(object sender, EventArgs e)
		{

		}

        private void btnEvaluate_Click(object sender, EventArgs e)
		{
            btnEvaluate.Visible = false;
            engine.StartThinking();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
			if (engine.gameInformation.fileName == "New Game")
            {
                SaveAs();
            }
            else
            {
                SaveFile saveFile = new SaveFile(engine.gameInformation.fileName, engine.BoardSize, engine.gameInformation.blackPlayerName,
                       engine.gameInformation.whitePlayerName, playedMoves);
            }
        }

        private void firstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.UndoAll();
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.Undo();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.Redo();
        }

        private void lastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            engine.RedoAll();
        }


        private PictureBox SelectedPictureBox
        {
            get
            {
                return (selectedPictureBox);
            }
            set
            {
                engine.Redraw();

                //select new 
                selectedPictureBox = value;

                if (value == null)
                {
                    selectToolStripMenuItem.Enabled = false;
                    selectedImage = null;
                    statusStrip1.Items[0].Visible = false;
                    return;
                }
                else
                {
                    selectToolStripMenuItem.Enabled = true;
                    selectedImage = value.Image;
                    statusStrip1.Items[0].Text = "Selected position = " + value.Tag.ToString();
                    statusStrip1.Items[0].Visible = true;
                }

                //modify bitmap - draw rectange
                Image image = new Bitmap(value.Image);
                Graphics g = Graphics.FromImage(image);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                Pen redPen = new Pen(Color.Red);
                Rectangle rect = new Rectangle(0, 0, image.Height - 1, image.Height - 1);
                g.DrawRectangle(redPen, rect);

                //and draw it back
                selectedPictureBox.Image = image;
            }
        }

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPictureBox.Tag.ToString() != "")
            {
				string notification = selectedPictureBox.Tag.ToString();

                SquareInfo squareInfo;

                engine.GetSquareInfo(notification, out squareInfo);

				string str = "Index = " + conversions.Square(notification);
				str += Environment.NewLine;

                str += Environment.NewLine + "symbol = " + squareInfo.symbol;
                str += Environment.NewLine;

                str += Environment.NewLine + "bothPlayerEvaluation = " + squareInfo.bothPlayerEvaluation;
                str += Environment.NewLine;

                str += Environment.NewLine + "blackPlayerEvaluation = " + squareInfo.blackPlayerEvaluation;
                str += Environment.NewLine + "whitePlayerEvaluation = " + squareInfo.whitePlayerEvaluation;
                str += Environment.NewLine;

                str += Environment.NewLine + "vct = " + squareInfo.vct;
                str += Environment.NewLine;

                str += Environment.NewLine;
                str += "blackEvaluation" + Environment.NewLine;
                for (int direction = 0; direction < 4; direction++)
                {
                    str += (Direction)direction + ": " + squareInfo.directionData[direction].evaluationBlack.ToString() + Environment.NewLine;
                }

                str += Environment.NewLine;
                str += "whiteEvaluation" + Environment.NewLine;
                for (int direction = 0; direction < 4; direction++)
                {
                    str += (Direction)direction + ": " + squareInfo.directionData[direction].evaluationWhite.ToString() + Environment.NewLine;
                }

                str += Environment.NewLine;
                str += "patterns" + Environment.NewLine;
                for (int direction = 0; direction < 4; direction++)
                {
                    str += (Direction)direction + ": " + squareInfo.directionData[direction].hash.ToString("X5") + Environment.NewLine;
                }

                MessageBox.Show(str, "Square " + notification);              
            }
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            SaveAs();
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
            if (info == null) return;

            //  show elapsed time
            listBox2.Items.Clear();
            listBox2.Items.Add("elapsed time = " + String.Format("{0:f1}s", info.elapsedTime.TotalSeconds));
            //  show number of evaluated moves
            if (info.examinedMoves >= 10000)
            {
                listBox2.Items.Add("moves = " + String.Format("{0}kN ({1:f1}kN/s)", info.examinedMoves / 1000, info.MovesPerSecond / 1000));
            }
            else
            {
                listBox2.Items.Add("moves = " + String.Format("{0}N ({1:f1}kN/s)", info.examinedMoves, info.MovesPerSecond / 1000));
            }
            // search result
            listBox2.Items.Add("winner = " + info.winner.ToString());

            // best move
            if (info.bestMove != null)
            {
                listBox2.Items.Add("best move = " + info.bestMove.ToString());
            }

            listBox2.Items.Add("TT hits = " + info.TThits.ToString("f1") + "%");
            listBox2.Items.Add("evaluation = " + info.evaluation.ToString());
            listBox2.Items.Add("reached depth = " + info.reachedDepth.ToString());

            if (info.nbCutoffs > 2000)
            {
                listBox2.Items.Add("cutoffs = " + String.Format("{0}k", info.nbCutoffs / 1000));
            }
            else
            {
                listBox2.Items.Add("cutoffs = " + String.Format("{0}", info.nbCutoffs));
            }

            if (info.nbVCTCutoffs > 2000)
            {
                listBox2.Items.Add("VCT cutoffs = " + String.Format("{0}k", info.nbVCTCutoffs / 1000));
            }
            else
            {
                listBox2.Items.Add("VCT cutoffs = " + String.Format("{0}", info.nbVCTCutoffs));
            }
        }

        void ThinkingFinished(SearchInformation info)
        {
            btnEvaluate.Visible = true;
            ThinkingProgress(info);
        }

        private void mnuShowPossibleMoves_Click(object sender, EventArgs e)
        {
            if (possibleMoves != null && possibleMoves.Visible)
            {
                possibleMoves.BringToFront();
            }
            else
            {
                possibleMoves = new PossibleMoves(engine);
                possibleMoves.Show();
            }
        }

        private void mnuShowBestLine_Click(object sender, EventArgs e)
        {
            bestLine.Show();
        }

        private void butResetTtable_Click(object sender, EventArgs e)
        {
			engine.ResetTtTable();
        }

		private void btnStop_Click(object sender, EventArgs e)
		{
			engine.StopThinking();
		}

		private void buttonBlack_Click(object sender, EventArgs e)
		{
            if (SelectedPictureBox.Tag.ToString() != "")
            {
				int square = conversions.Square(SelectedPictureBox.Tag.ToString());

				ABMove move = new ABMove(square, Player.BlackPlayer, engine.BoardSize,new TimeSpan());
                engine.MakeMove(move);
            }
		}

		private void buttonWhite_Click(object sender, EventArgs e)
		{
			if (SelectedPictureBox.Tag.ToString() != "")
			{
				int square = conversions.Square(SelectedPictureBox.Tag.ToString());

                ABMove move = new ABMove(square, Player.WhitePlayer, engine.BoardSize, new TimeSpan());
				engine.MakeMove(move);
			}
		}

        private void heuristicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tuning heuristics = new Tuning(engine);
            heuristics.Show();
        }

        private void numericUpDownThinkTime_ValueChanged(object sender, EventArgs e)
        {
            engine.MaxThinkingTime = new TimeSpan(0, 0, 0, 0, (int)(1000 * numericUpDownThinkTime.Value));
        }

        private void numericUpDownDepth_ValueChanged(object sender, EventArgs e)
        {
            engine.MaxSearchDepth = (int)numericUpDownDepth.Value;
            engine.Redraw();
        }

        private void numericUpDownDfPnHash_ValueChanged(object sender, EventArgs e)
        {
            engine.TranspositionTableSize = (int)(1000000 * numericUpDownDfPnHash.Value);
        }

        private void checkBoxFixedDepth_CheckedChanged(object sender, EventArgs e)
        {
            engine.IterativeDeepening = !checkBoxFixedDepth.Checked;
        }

        private void panelBoard_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listViewGame_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

	}
}