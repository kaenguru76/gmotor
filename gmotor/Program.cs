using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GomokuEngine;
using System.Diagnostics;

namespace gmotor
{
    class Program
    {
        static Engine engine;
        static Conversions conversions;
        static List<ABMove> playedMoves;
        static SearchInformation searchInfo;

        static void Main(string[] args)
        {
            Match match;
            int timeoutMatch = 60000;
            int timeoutTurn = 3000;
            int timeLeft = timeoutMatch;
            int timePortion = 10;
            int movesPerGame = 35;

            engine = new Engine();
            engine.NewGameE += new Engine.NewGameEvent(engine_NewGameE);
            engine.MovesChangedE += new Engine.MovesChangedEvent(engine_MovesChanged);
            engine.ThinkingProgress += new Engine.ThinkingProgressEvent(engine_ThinkingProgress);
            engine.ThinkingFinished += new Engine.ThinkingFinishedEvent(engine_ThinkingFinished);

            for (; ; )
            {
                //  cti vstup
                String str = Console.ReadLine().ToUpper();

#if DEBUG
                Console.WriteLine("DEBUG -> " + str);
#endif
                //  START
                Regex rStart = new Regex(@"START\s*(\d+)");
                match = rStart.Match(str);
                if (match.Success)
                {
                    int boardSize = Convert.ToInt32(match.Groups[1].Value);
                    if (boardSize >= 15 && boardSize <= 20)
                    {
                        conversions = new Conversions(boardSize);

                        //say to engine, that it is new game
                        engine.gameInformation.fileName = "gmotor" + engine.Version;
                        engine.NewGame(boardSize);
                    }
                    else
                    {
                        Console.WriteLine("ERROR Only board sizes between 15 and 20 supported!");
                    }
                    continue;
                }

                if (str == "END")
                {
                    return;
                }

                if (str == "ABOUT")
                {
                    Console.WriteLine("name=\"gmotor\", version=\"2014\", author=\"Roman Vanèura\", country=\"Czech Republic\""+
                        ", email=\"roman.vancura@email.cz\"");
                    continue;
                }

                //  TURN
                Regex rTurn = new Regex(@"TURN\s*(\d+),(\d+)");
                match = rTurn.Match(str);
                if (match.Success)
                {
                    int row = Convert.ToInt32(match.Groups[1].Value);
                    int column = Convert.ToInt32(match.Groups[2].Value);
                    if (row >= 0 && row <= engine.BoardSize && column >= 0 && column < engine.BoardSize)
                    {
                        ABMove move = new ABMove(conversions.RowAndColumn2Index(row, column), engine.WhoIsOnMove, engine.BoardSize, new TimeSpan());
                        engine.MakeMove(move);

#if DEBUG
                        Console.WriteLine("DEBUG oponent: " + move.ToString());
#endif
                        engine.MaxThinkingTime = new TimeSpan(0, 0, 0, 0, Math.Min(Math.Min(timeoutTurn, timeLeft / timePortion), timeoutMatch / movesPerGame));
#if DEBUG
                        Console.WriteLine("DEBUG time: " + engine.MaxThinkingTime.TotalSeconds.ToString() + "s");
#endif
                        engine.StartThinking();
                    }
                    else
                    {
                        Console.WriteLine("ERROR Wrong move!");
                    }
                    continue;
                }

                if (str == "BEGIN")
                {
                    engine.MaxThinkingTime = new TimeSpan(0, 0, 0, 0, 0);
                    engine.StartThinking();
                    continue;
                }

                if (str == "BOARD")
                {
                    for (; ; )
                    {
                        //  cti vstup
                        String str1 = Console.ReadLine().ToUpper();
#if DEBUG
						Console.WriteLine("DEBUG -> " + str1);
#endif
                        //  BOARD
                        Regex rBoard = new Regex(@"(\d+),(\d+),(\d+)");
                        Match match1 = rBoard.Match(str1);
                        if (match1.Success)
                        {
                            int row = Convert.ToInt32(match1.Groups[1].Value);
                            int column = Convert.ToInt32(match1.Groups[2].Value);
                            int who = Convert.ToInt32(match1.Groups[3].Value);
                            Player player = (who == 1) ? Player.BlackPlayer : Player.WhitePlayer;
                            ABMove move = new ABMove(conversions.RowAndColumn2Index(row, column), player, engine.BoardSize, new TimeSpan());
                            engine.MakeMove(move);
#if DEBUG
                            Console.WriteLine("DEBUG -> " + move.ToString());
#endif
                            continue;
                        }

                        if (str1.ToUpper() == "DONE")
                        {
                            engine.MaxThinkingTime = new TimeSpan(0, 0, 0, 0, Math.Min(Math.Min(timeoutTurn, timeLeft / timePortion), timeoutMatch / movesPerGame));
                            engine.StartThinking();
                            break;
                        }
                    }
                    continue;
                }

                //  INFO
                Regex rInfo = new Regex(@"INFO\s*(\w+)\s+([-]?\w+)");
                match = rInfo.Match(str);
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    if (key == "TIMEOUT_MATCH")
                    {
                        timeoutMatch = Convert.ToInt32(match.Groups[2].Value);
                    }
                    else if (key == "TIMEOUT_TURN")
                    {
                        timeoutTurn = Convert.ToInt32(match.Groups[2].Value);
                    }
                    else if (key == "TIME_LEFT")
                    {
                        timeLeft = Convert.ToInt32(match.Groups[2].Value);
                    }
                    continue;
                }

                if (str == "RESTART")
                {
                    if (engine.BoardSize > 0)
                    {
                        engine.gameInformation.fileName = "gmotor" + engine.Version;
                        engine.NewGame(engine.BoardSize);
                        continue;
                    }
                }

                Console.WriteLine("ERROR Unsupported command:{0}", str);
            }
        }

        static void engine_ThinkingProgress(SearchInformation info)
        {
        }

        static void engine_MovesChanged(GameInformation gameInformation)
        {
            playedMoves = gameInformation.playedMoves;
        }

        static void engine_NewGameE()
        {
            Console.WriteLine("OK");
#if DEBUG
            Console.WriteLine("DEBUG <- Initialized");
#endif
        }

        static void engine_ThinkingFinished(SearchInformation info)
        {
            searchInfo = info;
            ThinkingFinished();
        }

        static void ThinkingFinished()
        {
            engine.VctActive = false;
            engine.MakeMove(searchInfo.principalVariation[0]);

			Console.WriteLine(String.Format("MESSAGE time={0:f2}s, depth={1}, nodes={2} ({3:f1}kN/s), evaluation={4}, pv={5}",
                searchInfo.elapsedTime.TotalMilliseconds / 1000, searchInfo.depth, 
                (searchInfo.examinedMoves >= 2000) ? (searchInfo.examinedMoves / 1000).ToString()+"kN" : searchInfo.examinedMoves.ToString()+"N",
                searchInfo.MovesPerSecond / 1000, EvaluationConstants.Score2Text(searchInfo.evaluation), searchInfo.PrincipalVariationText));

            string outputString = String.Format("{0},{1}", searchInfo.principalVariation[0].Row, searchInfo.principalVariation[0].Column);
            Console.WriteLine(outputString);
        }
    }
}


