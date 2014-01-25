using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using GomokuEngine;

namespace gvisu
{
    public partial class Tuning : Form
    {
        Engine engine;
        TuningInfo basicHeuristicsInfo;

        int phase;
        int row;
        int column;
        int bestPhase;

        public Tuning(Engine engine)
        {
            InitializeComponent();
   			this.engine = engine;
            engine.ThinkingProgress += new Engine.ThinkingProgressEvent(engine_ThinkingProgress);
            engine.ThinkingFinished +=new Engine.ThinkingFinishedEvent(engine_ThinkingFinished);

        }

        private void Heuristics_Load(object sender, EventArgs e)
        {

            basicHeuristicsInfo = engine.GetTuningInfo();

            //dataGridView1.Rows.Add("ThreatBonus", basicHeuristicsInfo.threatBonus);

            for (int row = 0; row < basicHeuristicsInfo.scoreEvaluation.Length; row++)
            {
                dataGridView1.Rows.Add((BothPlayerEvaluation)row, basicHeuristicsInfo.scoreEvaluation[row]);
            }

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
            //  show elapsed time
            listBox1.Items.Clear();
            listBox1.Items.Add("Elapsed Time = " + String.Format("{0:f1}", info.elapsedTime.TotalSeconds));
            //  show number of evaluated moves
            if (info.examinedMoves > 2000)
            {
                listBox1.Items.Add("Examined moves = " + String.Format("{0}k ({1:f1}k/s)", info.examinedMoves / 1000, info.MovesPerSecond / 1000));
            }
            else
            {
                listBox1.Items.Add("Examined moves = " + String.Format("{0} ({1:f1}k/s)", info.examinedMoves, info.MovesPerSecond / 1000));
            }
        }

        void SetParameters()
        {
            //Int32.TryParse(dataGridView1[1, 0].Value.ToString(), out basicHeuristicsInfo.threatHandicap);

            //store heuristics to engine
            for (int row = 0; row < basicHeuristicsInfo.scoreEvaluation.Length; row++)
            {
                DataGridViewCell dataGridViewCell = dataGridView1[1, row];
                Int32.TryParse(dataGridViewCell.Value.ToString(), out basicHeuristicsInfo.scoreEvaluation[row]);
            }

            engine.SetTuningInfo(basicHeuristicsInfo);
        }

        private void buttonTune_Click(object sender, EventArgs e)
        {
            buttonTune.Visible = false;

            //erase table
            for (int row = 0; row < basicHeuristicsInfo.scoreEvaluation.Length - 2; row++)
            {
                dataGridView1[2, row].Value = "";
                dataGridView1[3, row].Value = "";
            }
            textBoxActualMoves.Text = "";

            SetParameters();

            phase = 0;
            bestPhase = 0;

            //start
            engine.StartThinking();
        }

        void ThinkingFinished(SearchInformation info)
        {
            ThinkingProgress(info);

            if (buttonTune.Visible) return;

            if (phase == 0)
            {
                textBoxActualMoves.Text = info.examinedMoves.ToString();
                textBoxBestMoves.Text = info.examinedMoves.ToString();
            }
            else
            {
                dataGridView1[column + 2, row].Value = info.examinedMoves;
            }

            if (info.examinedMoves < int.Parse(textBoxBestMoves.Text))
            {
                textBoxBestMoves.Text = info.examinedMoves.ToString();
                bestPhase = phase-1;
            }

            TuningInfo heuristicsInfo;

            ModifyTuningParameters(phase, out row, out column, out heuristicsInfo);

            if (row == -1 || column == -1)
            {
                if (int.Parse(textBoxBestMoves.Text) == int.Parse(textBoxActualMoves.Text))
                {
                    buttonTune.Visible = true;
                    return;
                }

                ModifyTuningParameters(bestPhase, out row, out column, out heuristicsInfo);

                basicHeuristicsInfo = new TuningInfo(heuristicsInfo);

                //dataGridView1[1, 1].Value = basicHeuristicsInfo.threatHandicap;

                for (int row1 = 0; row1 < basicHeuristicsInfo.scoreEvaluation.Length; row1++)
                {
                    dataGridView1[1, row1].Value = basicHeuristicsInfo.scoreEvaluation[row1];
                }

                //erase table
                for (int row1 = 0; row1 < dataGridView1.RowCount; row1++)
                {
                    dataGridView1[2, row1].Value = "";
                    dataGridView1[3, row1].Value = "";
                }

                textBoxActualMoves.Text = "";

                engine.SetTuningInfo(basicHeuristicsInfo);

                phase = 0;
                bestPhase = 0;

                //start
                engine.StartThinking();
                return;
            }

            engine.SetTuningInfo(heuristicsInfo);
            phase++;
            Thread.Sleep(100);
            engine.StartThinking();
        }

        void ModifyTuningParameters(int phase, out int row, out int column, out TuningInfo heuristicsInfo)
        {
            //make copy
            heuristicsInfo = new TuningInfo(basicHeuristicsInfo);

            if (phase < 2 * ((int)BothPlayerEvaluation.rest - (int)BothPlayerEvaluation.four_attacking)) 
            {
                row = phase / 2 + (int)BothPlayerEvaluation.four_attacking;
                column = phase % 2;
                if (column == 0)
                {
                    DecreaseValue(ref heuristicsInfo.scoreEvaluation[row]);
                }
                else
                {
                    IncreaseValue(ref heuristicsInfo.scoreEvaluation[row]);
                }
                goto L1;
            }

            //not existing phase
            row = -1;
            column = -1;
            return ;
L1:
            //check bounds
            for (int index1 = 0; index1 < heuristicsInfo.scoreEvaluation.Length; index1++)
            {
                if (heuristicsInfo.scoreEvaluation[index1] < -200)
                    heuristicsInfo.scoreEvaluation[index1] = -200;
                if (heuristicsInfo.scoreEvaluation[index1] > 200)
                    heuristicsInfo.scoreEvaluation[index1] = 200;
            }
        }

        private void buttonOnce_Click(object sender, EventArgs e)
        {
            SetParameters();
            engine.StartThinking();
        }

        private void DecreaseValue(ref int value)
        {
            int delta = value * 1/10;
            if (delta == 0) delta = 1;
            value = value - delta;
            //if (value <= 0) value = 0;
        }

        private void IncreaseValue(ref int value)
        {
            int delta = value * 1 / 10;
            if (delta == 0) delta = 1;
            value = value + delta;
        }

    }
}
