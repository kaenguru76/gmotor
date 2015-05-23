/*
 * Created by SharpDevelop.
 * User: Vančurovi
 * Date: 23.5.2015
 * Time: 23:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

using GomokuEngine;

namespace gvisu
{
	/// <summary>
	/// Description of MoveList.
	/// </summary>
	public partial class MoveList : UserControl
	{
		List<BoardSquare> playedMoves;
		int highlightedMove;

		public MoveList()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		
		void MoveListMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
            {
 				int positionToSearch = richTextBox1.GetCharIndexFromPosition(new Point(e.X, e.Y));
 				for(int move = 0; move < moveEndsAt.Length; move++){
 					if (positionToSearch < moveEndsAt[move]){
 						HighlightedMove = move;
 						break;
 					}
 				}
 					
			}
		}
		
		void AppendVisibleChars(StringBuilder sb1, string str1, ref int visibleChars)
		{
			sb1.Append(str1);
			visibleChars += str1.Length;
		}
		                        
		int[] moveEndsAt;
		
		public void SetMoves(List<BoardSquare> playedMoves)
		{
			this.playedMoves = playedMoves;
			RefreshMoveList();
		}
		
		void RefreshMoveList()
		{
			var sb1 = new StringBuilder(@"{\rtf1\ansi{\colortbl;\red255\green255\blue0;}");
			int visibleChars = 0;
			moveEndsAt = new int[playedMoves.Count];
			
			for (int i = 0; i < playedMoves.Count; i++) {
				if (i==highlightedMove) sb1.Append(@" \highlight1 ");
				sb1.Append(@" \b ");
				AppendVisibleChars(sb1, Convert.ToString(i/2 + 1) + @".", ref visibleChars);
				sb1.Append(@" \b0 ");
				AppendVisibleChars(sb1, playedMoves[i].ToString(), ref visibleChars);
				if (i==highlightedMove) sb1.Append(@" \highlight0 ");
				moveEndsAt[i] = visibleChars;
				if (++i < playedMoves.Count) {
					AppendVisibleChars(sb1, " " + playedMoves[i].ToString(), ref visibleChars);
					moveEndsAt[i] = visibleChars;
				}
			}
			sb1.Append(@"}");	
			richTextBox1.Rtf = sb1.ToString();	
		}
		
		int HighlightedMove {
			get{
				return highlightedMove;
			}
			set{
				highlightedMove = value;
				RefreshMoveList();
			}	
		}
	}
}
