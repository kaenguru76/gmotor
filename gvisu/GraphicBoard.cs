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

namespace gvisu
{
	/// <summary>
	/// Description of GraphicBoard.
	/// </summary>
	public partial class GraphicBoard : UserControl
	{
		Bitmap stones;
		Bitmap stoneEmpty;
		
		int boardSize;
		GomokuEngine.Conversions conversions;
		
		public GraphicBoard()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			BoardSize = 20;
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
    		
			for (int column = 0; column < boardSize; column++) {
				PointF coord = GetSquareCoordinates(-1, column);
				g.DrawString(conversions.Column(column), font, brush, coord.X+4, coord.Y);
			}

			//draw vertical legend
			for (int row = 0; row < boardSize; row++) {
				PointF coord = GetSquareCoordinates(row, -1);
				g.DrawString(conversions.Row(row), font, brush, coord.X, coord.Y+1);
			}

			//draw board
			for (int column = 0; column < boardSize; column++) {
				for (int row = 0; row < boardSize; row++) {
					g.DrawImage(stoneEmpty, GetSquareCoordinates(row, column));
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
			
		}
		
		PointF GetSquareCoordinates(int row, int column)
		{
			var point = new PointF((column + 1) * stoneEmpty.Width, (boardSize - row - 1) * stoneEmpty.Height);
			return point;
		}
		
		public int BoardSize {
			set {
				boardSize = value;
				conversions = new GomokuEngine.Conversions(boardSize);
			}
		}
	}
}
