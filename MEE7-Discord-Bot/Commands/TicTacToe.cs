﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class TicTacToe : Command
    {
        List<TicTacToeGame> Games = new List<TicTacToeGame>();

        public TicTacToe() : base("tictactoe", "Play TicTacToe against random people!", false)
        {

        }

        public override void Execute(SocketMessage commandmessage)
        {
            if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length < 2 || commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "help")
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddFieldDirectly(Prefix + CommandLine + " newGame + a mentioned user", "Creates a new game against the mentioned user");
                Embed.AddFieldDirectly(Prefix + CommandLine + " set + coordinates", "Sets your symbol at the specified coordinates in the form of \"1,2\" " +
                    "no spaces allowed eg. " + Prefix + CommandLine + " set 2,3\nWarning for Computer Science people: coordinates start at 1!");
                Embed.AddFieldDirectly(Prefix + CommandLine + " game", "Prints the game you are currently in");
                Embed.WithDescription("TicTacToe Commands:");
                Program.SendEmbed(Embed, commandmessage.Channel).Wait();
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "newGame")
            {
                if (commandmessage.MentionedUsers.Count < 1 || commandmessage.MentionedUsers.Count > 1)
                {
                    Program.SendText("You need exactly one player to play against!", commandmessage.Channel).Wait();
                }
                else
                {
                    if (Games.Exists(x => x.Player1 == commandmessage.MentionedUsers.ElementAt(0)) || Games.Exists(x => x.Player2 == commandmessage.MentionedUsers.ElementAt(0)))
                    {
                        Program.SendText(commandmessage.MentionedUsers.ElementAt(0).Mention + " is already in a game.", commandmessage.Channel).Wait();
                    }
                    else if (Games.Exists(x => x.Player1 == commandmessage.Author) || Games.Exists(x => x.Player2 == commandmessage.Author))
                    {
                        Program.SendText("You are already in a game.", commandmessage.Channel).Wait();
                    }
                    else
                    {
                        if (commandmessage.MentionedUsers.ElementAt(0).IsBot)
                        {
                            if (commandmessage.MentionedUsers.ElementAt(0).Id == Program.GetSelf().Id)
                                Program.SendText("You will be able to play against me once my master teaches me the game!", commandmessage.Channel).Wait();
                            else
                                Program.SendText("You cant play with a bot!", commandmessage.Channel).Wait();
                        }
                        else
                        {
                            if (commandmessage.MentionedUsers.ElementAt(0).Id == commandmessage.Author.Id)
                                Program.SendText("You can't play against yourself!", commandmessage.Channel).Wait();
                            else
                            {
                                Games.Add(new TicTacToeGame(commandmessage.MentionedUsers.ElementAt(0), commandmessage.Author));
                                Program.SendText("Created new game against " + commandmessage.MentionedUsers.ElementAt(0) + " successfully!", commandmessage.Channel).Wait();
                                Program.SendText(Games.Last().ToString(), commandmessage.Channel).Wait();
                            }
                        }
                    }
                }
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "set")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == commandmessage.Author || x.Player2 == commandmessage.Author);
                }
                catch { }

                if (Game == null)
                {
                    Program.SendText("You are not in a game!", commandmessage.Channel).Wait();
                }
                else
                {
                    if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length < 3)
                    {
                        Program.SendText("Where are the coordinates?!", commandmessage.Channel).Wait();
                    }
                    else
                    {
                        if (commandmessage.Author == Game.Player1 && !Game.Player1sTurn || commandmessage.Author == Game.Player2 && Game.Player1sTurn)
                        {
                            Program.SendText("Its not your turn!", commandmessage.Channel).Wait();
                        }
                        else
                        {
                            byte x = 255, y = 255;
                            try
                            {
                                string coords = commandmessage.Content.Split(new char[] { ' ', '\n' })[2];
                                string[] xy = coords.Split(',');
                                x = Convert.ToByte(xy[0]);
                                y = Convert.ToByte(xy[1]);

                                if (x == 0 || y == 0)
                                {
                                    Program.SendText("You cant put your symbol there!\nRemember Coordinates start at 1, not 0.", commandmessage.Channel).Wait();
                                    return;
                                }

                                x--;
                                y--;
                            }
                            catch
                            {
                                Program.SendText("The coordinates Mason what do they mean?!", commandmessage.Channel).Wait();
                            }

                            if (Game.Field[x, y] == 0 && x < 3 && y < 3)
                            {
                                Game.Player1sTurn = !Game.Player1sTurn;
                                if (commandmessage.Author == Game.Player1)
                                    Game.Field[x, y] = 1;
                                else
                                    Game.Field[x, y] = 2;

                                Program.SendText(Game.ToString(), commandmessage.Channel).Wait();

                                if (Game.Draw())
                                {
                                    Program.SendText("Draw between " + Game.Player1.Mention + " and " + Game.Player2.Mention + "!", commandmessage.Channel).Wait();
                                    Games.Remove(Game);
                                }

                                SocketUser Won = Game.PlayerWon();
                                if (Won == Game.Player1)
                                {
                                    Program.SendText("The meatbag called " + Game.Player1.Mention + " won!", commandmessage.Channel).Wait();
                                    Games.Remove(Game);
                                }
                                else if (Won == Game.Player2)
                                {
                                    Program.SendText("The meatbag called " + Game.Player2.Mention + " won!", commandmessage.Channel).Wait();
                                    Games.Remove(Game);
                                }
                            }
                            else
                            {
                                Program.SendText("You cant put your symbol there!", commandmessage.Channel).Wait();
                            }
                        }
                    }
                }
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "game" || commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "Game")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == commandmessage.Author || x.Player2 == commandmessage.Author);
                }
                catch { }

                if (Game == null)
                    Program.SendText("You are in no game!", commandmessage.Channel).Wait();
                else
                    Program.SendText(Game.ToString(), commandmessage.Channel).Wait();
            }
        }
    }
    public class TicTacToeGame
    {
        public bool Player1sTurn = true;
        public SocketUser Player1, Player2;
        public byte[,] Field = new byte[3, 3];

        public TicTacToeGame(SocketUser Player1, SocketUser Player2)
        {
            this.Player1 = Player1;
            this.Player2 = Player2;
        }

        public bool Draw()
        {
            bool Draw = true;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (Field[x, y] == 0)
                        Draw = false;
            return Draw;
        }
        public SocketUser PlayerWon()
        {
            byte p1 = 0, p2 = 0;
            for (int x = 0; x < 3; x++)
            {
                p1 = 0; p2 = 0;
                for (int y = 0; y < 3; y++)
                {
                    if (Field[x, y] == 1)
                        p1++;
                    else if (Field[x, y] == 2)
                        p2++;
                }
                if (p1 == 3)
                    return Player1;
                if (p2 == 3)
                    return Player2;
            }

            for (int y = 0; y < 3; y++)
            {
                p1 = 0; p2 = 0;
                for (int x = 0; x < 3; x++)
                {
                    if (Field[x, y] == 1)
                        p1++;
                    else if (Field[x, y] == 2)
                        p2++;
                }
                if (p1 == 3)
                    return Player1;
                if (p2 == 3)
                    return Player2;
            }

            p1 = 0; p2 = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Field[i, i] == 1)
                    p1++;
                else if (Field[i, i] == 2)
                    p2++;
            }
            if (p1 == 3)
                return Player1;
            if (p2 == 3)
                return Player2;

            p1 = 0; p2 = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Field[2 - i, i] == 1)
                    p1++;
                else if (Field[2 - i, i] == 2)
                    p2++;
            }
            if (p1 == 3)
                return Player1;
            if (p2 == 3)
                return Player2;

            return null;
        }

        public override string ToString()
        {
            string re = "";

            re += Player1.Mention + " (X) vs. " + Player2.Mention + " (O)\n\n";

            re += "```\n";
            re += "╔═══╦═══╦═══╗\n";
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (Field[x, y] == 0)
                        re += "║   ";
                    else if (Field[x, y] == 1)
                        re += "║ X ";
                    else if (Field[x, y] == 2)
                        re += "║ O ";
                }
                if (y != 3 - 1)
                    re += "║\n╠═══╬═══╬═══╣\n";
                else
                    re += "║\n╚═══╩═══╩═══╝\n";
            }
            re += "\n```";

            return re;
        }
    }
}
