using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using GomokuEngine;


namespace gvisu
{
	public partial class Main : Form
	{
		Engine engine;
		//ImageList BoardImageList;
		//ImageList BoardImageListWithPoint;
		//ImageList SemaforImageList;
		ImageList ToolbarImageList;
		//PictureBox[,] picSquares;
		//Image selectedImage;
		//PictureBox selectedPictureBox;

		//Conversions conversions;
		PossibleMoves possibleMoves;
		Evaluation evaluation;
        
		//List<ABMove> playedMoves;

		public Main()
		{
			InitializeComponent();

			engine = new Engine();
			engine.NewGameE += engine_NewGameE; 
			//engine.MovesChangedE += new Engine.MovesChangedEvent(engine_MovesChanged);
			engine.ThinkingFinished += engine_ThinkingFinished;
			engine.ThinkingProgress += engine_ThinkingProgress;
			engine.FileNameChanged += engine_FileNameChanged;
			engine.BlackPlayerNameChanged += engine_BlackPlayerNameChanged;
			engine.WhitePlayerNameChanged += engine_WhitePlayerNameChanged;
            
			graphicBoard1.SquareClicked += graphicBoard1_SquareClicked; 
            
			engine.NewGame(20);
		}

		void engine_FileNameChanged()
		{
			string noPath = Path.GetFileName(engine.FileName);
			Text = noPath + " - " + Application.ProductName;
		}

		void engine_BlackPlayerNameChanged()
		{
			lblBlack.Text = engine.BlackPlayerName;
		}

		void engine_WhitePlayerNameChanged()
		{
			lblWhite.Text = engine.WhitePlayerName;
		}

		void graphicBoard1_SquareClicked(BoardSquare selectedSquare, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				engine.MakeMove(selectedSquare);
			}
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
            
		}

		void engine_NewGameE()
		{
			//PictureBox picSquare;
			//Label lblRow;
			//Label lblColumn;

			//conversions = new Conversions(engine.BoardSize);

			//load wood image list
//			Bitmap bitmaps = Properties.Resources.toolbar;
//			BoardImageList = new ImageList();
//			BoardImageList.ImageSize = new Size(bitmaps.Size.Height, bitmaps.Size.Height);
//			BoardImageList.Images.AddStrip(bitmaps);
//
//            //modify bitmaps - draw point
//            Bitmap bmpWithPoint = new Bitmap(bitmaps);
//            Graphics gWithPoint = Graphics.FromImage(bmpWithPoint);
//            gWithPoint.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
//            Brush whiteBrush = new SolidBrush(Color.White);
//            for (int index = 0; index < BoardImageList.Images.Count; index++)
//            {
//                Rectangle rect = new Rectangle(bmpWithPoint.Height/2 - 1 + index*(bmpWithPoint.Height), bmpWithPoint.Height/2 - 1, 3, 3);
//                gWithPoint.FillRectangle(whiteBrush, rect);
//            }
//
//            //create wood image list with point
//            BoardImageListWithPoint = new ImageList();
//            BoardImageListWithPoint.ImageSize = BoardImageList.ImageSize;
//            BoardImageListWithPoint.Images.AddStrip(bmpWithPoint);
//
//            //load semafor image list
//            bitmaps = Properties.Resources.semafor;
//            SemaforImageList = new ImageList();
//            SemaforImageList.ImageSize = new Size(bitmaps.Size.Height, bitmaps.Size.Height);
//            SemaforImageList.Images.AddStrip(bitmaps);
//            
			// resize board
//			graphicBoard1.BoardSize = engine.BoardSize;

			// shift control panel
//			panelControl.Left = graphicBoard1.Right;
//			panelControl.Height = graphicBoard1.Height;

			//resize listViews
			//listViewGame.Left = panelBoard.Right;
//            listViewGame.Height = panelControl.Height - listViewGame.Top;


			// resize whole form
			//this.ClientSize = new Size(panelControl.Right, graphicBoard1.Bottom + statusStrip1.Height);


			//set back colour
			//panelBoard.BackColor = bitmaps.GetPixel(0, 0);


			//define square pictures
//			picSquares = new PictureBox[engine.BoardSize, engine.BoardSize];



//			//draw 2 symbols
//			buttonBlack.Image = BoardImageList.Images[1];
//			//buttonBlack.Size = BoardImageList.ImageSize;
//			buttonWhite.Image = BoardImageList.Images[2];
//			//buttonWhite.Size = BoardImageList.ImageSize;
//
//            //upgrade title
//            numericUpDownThinkTime.Value = (decimal)(engine.MaxThinkingTime.TotalMilliseconds / 1000);
//            numericUpDownDepth.Value = (decimal)(engine.MaxSearchDepth);
//            numericUpDownDfPnHash.Value = (decimal)(engine.TranspositionTableSize / 1000000);
//
//            //mnuShowBestLine.Enabled = true;
//            mnuShowPossibleMoves.Enabled = true;
//            heuristicsToolStripMenuItem.Enabled = true;
//            evaluationToolStripMenuItem.Enabled = true;
		}

		//        void engine_MovesChanged(GameInformation gameInformation)
		//        {
		//            this.playedMoves = gameInformation.playedMoves;
		//
		//            //erase board
		//            foreach (PictureBox pic in picSquares)
		//            {
		//                pic.Image = BoardImageList.Images[0];
		//            }
		//            listViewGame.Items.Clear();
		//
		//            for (int index = 0; index < playedMoves.Count; index++)
		//            {
		//                ABMove move = playedMoves[index];
		//
		//                string[] str = move.ToStringArray(index + 1);
		//                ListViewItem listViewItem = new ListViewItem(str);
		//
		//                listViewGame.Items.Add(listViewItem);
		//
		//                if (move.player == Player.BlackPlayer)
		//                {
		//                    picSquares[move.Row, move.Column].Image = BoardImageList.Images[1];
		//                }
		//                else
		//                {
		//                    picSquares[move.Row, move.Column].Image = BoardImageList.Images[2];
		//                }
		//            }
		//            //scroll listbox down
		//            if (listViewGame.Items.Count > 0)
		//            {
		//                listViewGame.TopItem = listViewGame.Items[listViewGame.Items.Count - 1];
		//            }
		//
		//            //make point on last move
		//            if (playedMoves.Count > 0)
		//            {
		//                ABMove move = playedMoves[playedMoves.Count - 1];
		//
		//                switch (move.player)
		//                {
		//                    case Player.None:
		//                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[0];
		//                        break;
		//
		//                    case Player.BlackPlayer:
		//                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[1];
		//                        break;
		//
		//                    case Player.WhitePlayer:
		//                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[2];
		//                        break;
		//                }
		//            }
		//
		//            //disable next move button, if there is no next move
		//            if (gameInformation.nextMove != null)
		//            {
		//                nextToolStripButton.Enabled = true;
		//                lastToolStripButton.Enabled = true;
		//                nextToolStripMenuItem.Enabled = true;
		//                lastToolStripMenuItem.Enabled = true;
		//            }
		//            else
		//            {
		//                nextToolStripButton.Enabled = false;
		//                lastToolStripButton.Enabled = false;
		//                nextToolStripMenuItem.Enabled = false;
		//                lastToolStripMenuItem.Enabled = false;
		//            }
		//
		//            //disable previous move button, if there is no previous move
		//            if (playedMoves.Count == 0)
		//            {
		//                firstToolStripButton.Enabled = false;
		//                previousToolStripButton.Enabled = false;
		//                firstToolStripMenuItem.Enabled = false;
		//                previousToolStripMenuItem.Enabled = false;
		//            }
		//            else
		//            {
		//                firstToolStripButton.Enabled = true;
		//                previousToolStripButton.Enabled = true;
		//                firstToolStripMenuItem.Enabled = true;
		//                previousToolStripMenuItem.Enabled = true;
		//            }
		//
		//            //point on next move
		//            if (gameInformation.nextMove != null)
		//            {
		//                picSquares[gameInformation.nextMove.Row, gameInformation.nextMove.Column].Image = BoardImageListWithPoint.Images[0];
		//            }
		//
		//            //refresh possible moves
		//            for (int index = 0; index < gameInformation.possibleMoves.Count; index++)
		//			{
		//                ABMove move = gameInformation.possibleMoves[index];
		//
		//				if (move.player == Player.BlackPlayer)
		//				{
		//                    if (gameInformation.nextMove != null && move.square == gameInformation.nextMove.square)
		//                    {
		//                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[3];
		//                    }
		//                    else
		//					{
		//						picSquares[move.Row, move.Column].Image = BoardImageList.Images[3];
		//					}
		//				}
		//				else
		//				{
		//                    if (gameInformation.nextMove != null && move.square == gameInformation.nextMove.square)
		//                    {
		//                        picSquares[move.Row, move.Column].Image = BoardImageListWithPoint.Images[4];
		//                    }
		//                    else
		//                    {
		//						picSquares[move.Row, move.Column].Image = BoardImageList.Images[4];
		//					}
		//				}
		//			}
		//
		//            statusStrip1.Items[1].Text = "Evaluation = " + EvaluationConstants.Score2Text(gameInformation.Evaluation);
		//            statusStrip1.Items[1].Visible = true;
		//
		//            statusStrip1.Items[2].Text = "Gain square = " + gameInformation.GainSquare;
		//            statusStrip1.Items[2].Visible = true;
		//        }

		void New()
		{
			//open window with parameters for new game
			var newGame = new NewGame();
			if (newGame.ShowDialog() == DialogResult.OK) {
				//say to engine, that it is new game
				engine.NewGame(newGame.BoardSize);
				//enable items
//                mnuSave.Enabled = true;
//                mnuSaveAs.Enabled = true;
//                mnuReopen.Enabled = true;
//                saveToolStripButton.Enabled = true;
			}
			//dispose dialog
			newGame.Close();
		}
		
		void Open()
		{
			var openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Load game";
			openFileDialog.Filter = "All gomoku files (*.psq;*.rec)|*.psq;*.rec|Piskvorky (*.psq)|*.psq|Piskvorky (*.rec)|*.rec";
			openFileDialog.FilterIndex = 1;

			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				if (openFileDialog.FileName != "") {
					int boardSize;
					string blackPlayerName;
					string whitePlayerName;
					List<BoardSquare> bs;
                	
					var openFile = new OpenFile(openFileDialog.FileName, out boardSize, out blackPlayerName, out whitePlayerName, out bs);
					if (openFile != null && bs == null)
						return;
					engine.FileName = openFileDialog.FileName;
					engine.BlackPlayerName = blackPlayerName;
					engine.WhitePlayerName = whitePlayerName;
					engine.NewGame(boardSize, bs);
					//enable items
					//mnuSave.Enabled = true;
//                    mnuSaveAs.Enabled = true;
//                    mnuReopen.Enabled = true;
//                    saveToolStripButton.Enabled = true;
				}
			}

		}

		private void SaveAs()
		{
			var saveFileDialog = new SaveFileDialog();

			saveFileDialog.Title = "Save " + engine.FileName;
			saveFileDialog.Filter = "Piskvorky (*.psq)|*.psq|Piskvorky (*.rec)|*.rec";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.AddExtension = true;
			saveFileDialog.OverwritePrompt = true;
			saveFileDialog.ValidateNames = true;
			saveFileDialog.FileName = engine.FileName;

			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				if (saveFileDialog.FileName != "") {
					var saveFile = new SaveFile(saveFileDialog.FileName, engine.BoardSize, engine.BlackPlayerName,
						               engine.WhitePlayerName, engine.PlayedMoves);
					//remember filename
//                    if (saveFile != null)
//                    {
					engine.FileName = saveFileDialog.FileName;
//
//						//upgrade title
//						Text = Application.ProductName + " - " + engine.gameInformation.fileName;
//					}
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
			string extension = Path.GetExtension(engine.FileName);
        	
			if (extension == "") {
				SaveAs();
			} else {
				var saveFile = new SaveFile(engine.FileName, engine.BoardSize, engine.BlackPlayerName,
					               engine.WhitePlayerName, engine.PlayedMoves);

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


		private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
//            if (SelectedPictureBox.Tag.ToString() != "")
//            {
//				string notification = selectedPictureBox.Tag.ToString();
//
//                SquareInfo squareInfo;
//
//                engine.GetSquareInfo(notification, out squareInfo);
//
//				string str = "Index = " + conversions.Square(notification);
//				str += Environment.NewLine;
//
//                str += Environment.NewLine + "symbol = " + squareInfo.symbol;
//                str += Environment.NewLine;
//
//                str += Environment.NewLine + "bothPlayerEvaluation = " + squareInfo.bothPlayerEvaluation;
//                str += Environment.NewLine;
//
//                str += Environment.NewLine + "blackPlayerEvaluation = " + squareInfo.blackPlayerEvaluation;
//                str += Environment.NewLine + "whitePlayerEvaluation = " + squareInfo.whitePlayerEvaluation;
//                str += Environment.NewLine;
//
//                str += Environment.NewLine + "vct = " + squareInfo.vct;
//                str += Environment.NewLine;
//
//                str += Environment.NewLine;
//                str += "blackEvaluation" + Environment.NewLine;
//                for (int direction = 0; direction < 4; direction++)
//                {
//                    str += (Direction)direction + ": " + squareInfo.directionData[direction].evaluationBlack.ToString() + Environment.NewLine;
//                }
//
//                str += Environment.NewLine;
//                str += "whiteEvaluation" + Environment.NewLine;
//                for (int direction = 0; direction < 4; direction++)
//                {
//                    str += (Direction)direction + ": " + squareInfo.directionData[direction].evaluationWhite.ToString() + Environment.NewLine;
//                }
//
//                str += Environment.NewLine;
//                str += "patterns" + Environment.NewLine;
//                for (int direction = 0; direction < 4; direction++)
//                {
//                    str += (Direction)direction + ": " + squareInfo.directionData[direction].hash.ToString("X5") + Environment.NewLine;
//                }
//
//                MessageBox.Show(str, "Square " + notification);              
//            }
		}

		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			SaveAs();
		}

		delegate void ThinkingProgressDelegate(SearchInformation info);

		void engine_ThinkingProgress(SearchInformation info)
		{
			if (this.IsHandleCreated) {
				ThinkingProgressDelegate delegate1 = new ThinkingProgressDelegate(ThinkingProgress);
				this.BeginInvoke(delegate1, info);
			}
		}

		delegate void ThinkingFinishedDelegate(SearchInformation info);

		void engine_ThinkingFinished(SearchInformation info)
		{
			if (this.IsHandleCreated) {
				ThinkingFinishedDelegate delegate1 = new ThinkingFinishedDelegate(ThinkingFinished);
				this.BeginInvoke(delegate1, info);
			}
		}

		void ThinkingProgress(SearchInformation info)
		{
			if (info == null)
				return;

			listBox2.Items.Clear();
			listBox2.Items.Add("elapsed time= " + String.Format("{0:f2}s", info.elapsedTime.TotalSeconds));
			listBox2.Items.Add("nodes= " + info.ExaminedMovesText);
			listBox2.Items.Add("speed= " + info.MovesPerSecondText);
			listBox2.Items.Add("evaluation= " + EvaluationConstants.Score2Text(info.evaluation));
			listBox2.Items.Add("depth= " + info.depth + " (" + info.deepestVctSearch + " VCT)");
			listBox2.Items.Add("PV= " + info.PrincipalVariationText);
			listBox2.Items.Add("cutoffs= " + info.CutoffsText);
			listBox2.Items.Add("TT hits= " + info.TtHits.ToString("f0") + "%");
		}

		void ThinkingFinished(SearchInformation info)
		{
			btnEvaluate.Visible = true;
			ThinkingProgress(info);
		}

		private void mnuShowPossibleMoves_Click(object sender, EventArgs e)
		{
			if (possibleMoves != null && possibleMoves.Visible) {
				possibleMoves.BringToFront();
			} else {
				possibleMoves = new PossibleMoves(engine);
				possibleMoves.Show();
			}
		}

		private void butResetTtable_Click(object sender, EventArgs e)
		{
			engine.ResetTtTable(checkBoxUseDictionary.Checked);
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			engine.StopThinking();
		}

		private void buttonBlack_Click(object sender, EventArgs e)
		{
//            if (SelectedPictureBox.Tag.ToString() != "")
//            {
//				int square = conversions.Square(SelectedPictureBox.Tag.ToString());
//
//				ABMove move = new ABMove(square, Player.BlackPlayer, engine.BoardSize,new TimeSpan());
//                engine.MakeMove(move);
//            }
		}

		private void buttonWhite_Click(object sender, EventArgs e)
		{
//			if (SelectedPictureBox.Tag.ToString() != "")
//			{
//				int square = conversions.Square(SelectedPictureBox.Tag.ToString());
//
//                ABMove move = new ABMove(square, Player.WhitePlayer, engine.BoardSize, new TimeSpan());
//				engine.MakeMove(move);
//			}
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
			//engine.Redraw();
		}

		private void numericUpDownDfPnHash_ValueChanged(object sender, EventArgs e)
		{
			engine.TranspositionTableSize = (int)(1000000 * numericUpDownDfPnHash.Value);
		}

		private void checkBoxFixedDepth_CheckedChanged(object sender, EventArgs e)
		{
			engine.IterativeDeepening = !checkBoxFixedDepth.Checked;
		}
		
		void EvaluationToolStripMenuItemClick(object sender, EventArgs e)
		{
			evaluation = new Evaluation();
			evaluation.Show();
		}
		

		

	}
}