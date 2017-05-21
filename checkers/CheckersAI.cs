﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    class CheckersAI
    {
        private Node decisionTree;
        private Move move;
        private int numOfMoves;
        private string me;
        private string opponent;
        private bool useMinmax;
        private int treeDepth;

        public CheckersAI(string colour, int treeDepth,bool flag)
        {
            this.me = colour;
            if (colour.Equals("red"))
            {
                opponent = "black";
            }
            else
            {
                opponent = "red";
            }
            this.treeDepth = treeDepth;
            numOfMoves = 0;
            useMinmax = flag;
        }

        public int getNumOfMoves()
        {
            return numOfMoves;
        }

        public bool useMinMax()
        {
            return useMinmax;
        }

        public Move getAiMove(Board board)
        {
            decisionTree = buildTree(board);
            move = pickMove();
            numOfMoves++;
            return move;
        }

        public string getPlayer()
        {
            return me;
        }

        private Node buildTree(Board board)
        {
            Node root = new Node(board, null, score(board,me));
            buildTree(root, treeDepth, true);
            return root;
        }

        private void buildTree(Node root, int depth, bool player)
        {
            if(depth == 0)
            {
                return;
            }

            string colour = player ? me : opponent; 

            List<Move> moves;
            if (root.getBoard().isJumped())
            {
                var lastMove = root.getMove();
                moves = root.getBoard().getJumpsFromLocation(colour,lastMove.RowToBeMovedTo,lastMove.ColToBeMovedTo);
            }
            else if(root.getBoard().getJumps(colour).Count > 0)
            {
                moves = root.getBoard().getJumps(colour);
            }
            else
            {
                moves = root.getBoard().getAllLegalMovesForColor(colour);
            }

            foreach (var move in moves)
            {
                Board copy = copyBoard(root.getBoard());
                copy.makeMove(move, colour);
                copy.handleJump(move, colour);
                Node child = new Node(copy, move, score(copy,colour));
                buildTree(child, depth - 1, !player);
                root.addChild(child);
            }


        }

        private Move pickMove()
        {
            if(useMinmax)
                minmax(decisionTree, treeDepth, true);
            else
                alphabeta(decisionTree, treeDepth, int.MinValue, int.MaxValue, true);

            int max = int.MinValue;
            int index = 0;
            for(int i = 0; i < decisionTree.getNumChildren(); i++)
            {
                Node child = decisionTree.getChild(i);
                if(child.getScore() >= max)
                {
                    max = child.getScore();
                    index = i;
                }
            }

            return decisionTree.getChild(index).getMove();
        }

        private int minmax(Node root, int depth, bool maxPlayer)
        {
            if (depth == 0 || root.getNumChildren() == 0)
                return root.getScore();

            if(maxPlayer)
            {
                int bestValue = int.MinValue;
                foreach (var child in root.getChildren())
                {
                    int val = minmax(child, depth - 1, false);
                    bestValue = Math.Max(bestValue, val);
                }
                root.setScore(bestValue);
                return bestValue;
            }
            else
            {
                int bestValue = int.MaxValue;
                foreach (var child in root.getChildren())
                {
                    int val = minmax(child, depth - 1, true);
                    bestValue = Math.Min(bestValue, val);
                }
                root.setScore(bestValue);
                return bestValue;
            }
        }

        private int alphabeta(Node root, int depth, int a, int b, bool maxPlayer)
        {
            if (depth == 0 || root.getNumChildren() == 0)
                return root.getScore();
            if(maxPlayer)
            {
                foreach (var child in root.getChildren())
                {
                    a = Math.Max(a, alphabeta(child, depth - 1, a, b, false));
                    if (a >= b)
                        break;
                }
                root.setScore(a);
                return a;
            }
            else
            {
                foreach (var child in root.getChildren())
                {
                    b = Math.Min(b, alphabeta(child, depth - 1, a, b, true));
                    if (a >= b)
                        break;
                }
                root.setScore(b);
                return b;
            }
        }

        private int score(Board board,string player)
        {
            int score = 150 * board.getAreasScore(player) + board.getBeatScore(player) + 300 * board.getLevelsScore(player);
            if (player.Equals("red"))
            {
                score += board.getRedWeightedScore() - board.getBeatScore("black");
                return score;
            }
            else
            {
                score += board.getBlackWeightedScore() - board.getBeatScore("red");
                return score;
            }
        }

        private Board copyBoard(Board board)
        {
            Board copy = new Board();

            for(int row = 0; row < 8; row++)
            {
                for(int col = 0; col < 8; col++)
                {
                    copy.AssignPiece(row, col, board.GetPiece(row, col));
                }
            }

            copy.setBlackCount(board.getBlackCount());
            copy.setBlackLadyCount(board.getBlackLadyCount());
            copy.setRedCount(board.getRedCount());
            copy.setRedLadyCount(board.getRedLadyCount());

            return copy;
        }


    }
}
