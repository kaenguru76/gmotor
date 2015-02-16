/*
 * Created by SharpDevelop.
 * User: Vančurovi
 * Date: 18.10.2014
 * Time: 22:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using GomokuEngine;

namespace gvisu
{
	/// <summary>
	/// Description of GraphicBoard.
	/// </summary>
	public partial class GraphicBoard : UserControl
	{
        public delegate void SquareClickedEH(BoardSquare selectedSquare, MouseEventArgs e);
        public event SquareClickedEH SquareClicked;

        Bitmap stones;
		Bitmap stoneEmpty;
		
		int boardSize;
		//GomokuEngine.Conversions conversions;
		BoardSquare selectedSquare;
		
		
		public GraphicBoard()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			boardSize = 20;
		}
		
		void GraphicBoardLoad(object sender, EventArgs e)
		{
			//ResourceManager resources = new ResourceManager("gvisu.GraphicBoard", Assembly.GetExecutingAssembly());
			//stones = (Bitmap)resources.GetObject("green");
			
			stones = Properties.Resources.green;
			stoneEmpty = stones.Clone(new Rectangle(0, 0, stones.Height, stones.Height), stones.PixelFormat);
/*
			string str1 = "./skins/wood.bmp";
			try {
				stones = new Bitmap(str1);				
			}
			catch (System.ArgumentException ex) {
//				// if (ex.Source != null)
				System.Diagnostics.Debug.Print(ex.ToString());
				MessageBox.Show(str1,"File not found",MessageBoxButtons.OK);
				//throw new System.Exception("File not found", ex);
			}
			//stones = new Bitmap(@"c:\Piskvorky\gmotor\gvisu\bin\Debug\skins\wood.bmp");
			*/
		}
		
		void GraphicBoardPaint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics; //get handle
			
			//draw horizontal legend
			var font = new Font(FontFamily.GenericSansSerif, 10);
			var brush = new SolidBrush(Color.Black);
			var bs = new BoardSquare(boardSize, 0, 0);
    		
			for (int column = 0; column < boardSize; column++) {
				g.DrawString(bs.ColumnToString(column), font, brush, GetSquareCoordinateX(column)+4, GetSquareCoordinateY(-1));
			}

			//draw vertical legend
			for (int row = 0; row < boardSize; row++) {
				g.DrawString(bs.RowToString(row), font, brush, GetSquareCoordinateX(-1), GetSquareCoordinateY(row)+1);
			}

			//draw board
			for (int column = 0; column < boardSize; column++) {
				for (int row = 0; row < boardSize; row++) {
					g.DrawImage(stoneEmpty, GetSquareCoordinateX(column), GetSquareCoordinateY(row));
//
//					picSquare = new System.Windows.Forms.PictureBox();
//					picSquare.Image = BoardImageList.Images[0];
//					picSquare.Name = "picSquare" + conversions.Complete(row , column);
//					picSquare.Size = BoardImageList.ImageSize;
//					picSquare.Top = panelBoard.Height - BoardImageList.ImageSize.Height - (row + 1) * picSquare.Size.Height;
//					picSquare.Left = BoardImageList.ImageSize.Width + column * picSquare.Size.Height;
//					picSquare.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
//					picSquare.TabStop = false;
//                    picSquare.Tag = conversions.Complete(row, column); 
//                    picSquare.MouseClick +=new MouseEventHandler(picSquare_MouseClick);
//					panelBoard.Controls.Add(picSquare);
//					picSquares[row, column] = picSquare;
				}
			}
			
			//draw selected square
			if (selectedSquare != null)
			{
				var redPen = new Pen(Color.Red);
				g.DrawRectangle(redPen, GetSquareCoordinateX(selectedSquare.Column), GetSquareCoordinateY(selectedSquare.Row), 
			                stoneEmpty.Width, stoneEmpty.Height);
			}
		}
		
	
		int GetSquareCoordinateX(int column)
		{
			return (column + 1) * stoneEmpty.Width;
		}

		int GetSquareCoordinateY(int row)
		{
			return (boardSize - row - 1) * stoneEmpty.Height;
		}
		
//		public int BoardSize {
//			set {
//				boardSize = value;
//				conversions = new GomokuEngine.Conversions(boardSize);
//			}
//		}
		void GraphicBoardClick(object sender, EventArgs e)
		{
			//determine where was the click
			var me = e as MouseEventArgs;
			int column;
			int row;
			
			//determine column
			for (column = 0; column <= boardSize; column++) {
				int X = GetSquareCoordinateX(column);
				if (X > me.X) {
					column--;
					break;
				}
			}

			//determine row
			for (row = -1; row <= boardSize; row++) {
				int Y = GetSquareCoordinateY(row);
				if (Y < me.Y) {
					break;
				}
			}
			
			//selectedSquare square changed
			SelectedSquare = new BoardSquare(boardSize, row, column);
			
			//left click => playmove
			//if (me.Button == MouseButtons.Left)
			//{
				SquareClicked(SelectedSquare, me);
			//}
		}
		
		BoardSquare SelectedSquare {
			get{
				return selectedSquare;
			}
			set{
				selectedSquare = value;
				Refresh(); //redraw board
			}		
		}

			
//                engine.Redraw();
//
//                //select new 
//                selectedPictureBox = value;
//
//                if (value == null)
//                {
//                    selectToolStripMenuItem.Enabled = false;
//                    selectedImage = null;
//                    statusStrip1.Items[0].Visible = false;
//                    return;
//                }
//                else
//                {
//                    selectToolStripMenuItem.Enabled = true;
//                    selectedImage = value.Image;
//                    statusStrip1.Items[0].Text = "Selected position = " + value.Tag.ToString();
//                    statusStrip1.Items[0].Visible = true;
//                }
//
//                //modify bitmap - draw rectange
//                Image image = new Bitmap(value.Image);
//                Graphics g = Graphics.FromImage(image);
//                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
//                Pen redPen = new Pen(Color.Red);
//                Rectangle rect = new Rectangle(0, 0, image.Height - 1, image.Height - 1);
//                g.DrawRectangle(redPen, rect);
//
//                //and draw it back
//                selectedPictureBox.Image = image;
//            }


	}
}
