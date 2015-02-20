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
using GomokuEngine;
using System.Collections.Generic;

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
		Bitmap stoneBlack;
		Bitmap stoneWhite;
		
		int boardSize;
		//GomokuEngine.Conversions conversions;
		BoardSquare selectedSquare;
		
		List<BoardSquare> _playedMoves;
			
		
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
			stoneEmpty = stones.Clone(new Rectangle(stones.Width*0/3, 0, stones.Height, stones.Height), stones.PixelFormat);
			stoneBlack = stones.Clone(new Rectangle(stones.Width*1/3, 0, stones.Height, stones.Height), stones.PixelFormat);
			stoneWhite = stones.Clone(new Rectangle(stones.Width*2/3, 0, stones.Height, stones.Height), stones.PixelFormat);

		}
		
		void GraphicBoardPaint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics; //get handle
			
			//draw horizontal legend
//			var font = new Font(FontFamily.GenericSansSerif, 10);
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

			//draw empty board
			for (int column = 0; column < boardSize; column++) {
				for (int row = 0; row < boardSize; row++) {
					g.DrawImage(stoneEmpty, GetSquareCoordinateX(column), GetSquareCoordinateY(row));
				}
			}
			
			//draw stones
			if (_playedMoves != null) {
				for (int i = 0; i < _playedMoves.Count; i++) {
					if (i % 2 == 0)
					{
						g.DrawImage(stoneBlack, GetSquareCoordinateX(_playedMoves[i].Column), GetSquareCoordinateY(_playedMoves[i].Row));
					}
					else
					{
						g.DrawImage(stoneWhite, GetSquareCoordinateX(_playedMoves[i].Column), GetSquareCoordinateY(_playedMoves[i].Row));
					}
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

		public void SetBoard (List<BoardSquare> playedMoves) {
			_playedMoves = playedMoves;
			Refresh();
		}
			



	}
}
