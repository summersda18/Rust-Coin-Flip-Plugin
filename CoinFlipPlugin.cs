using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Oxide.Core.Configuration;
using Oxide.Game.Rust.Cui;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;


namespace Oxide.Plugins
{
    [Info("Testing", "Lyle", "0.0.1")]

    class CoinFlipPlugin : RustPlugin
    {
        #region Structures
        private struct ActiveCoinFlip
        {
            internal int bet;
            internal BasePlayer target;
            internal int side;

            public ActiveCoinFlip(int bet, BasePlayer target, int side)
            {
                this.bet = bet;
                this.target = target;
                this.side = side;
            }

            public string show() { return target.UserIDString + " " + bet.ToString() + " " + side.ToString(); }
        }


        #endregion

        #region Data
        //References
        [PluginReference]
        Plugin Economics;

        //All active coinflips
        private Dictionary<string, ActiveCoinFlip> coinFlips = new Dictionary<string, ActiveCoinFlip>();
        #endregion

        private static System.Random random = new System.Random();

        #region Chat Commands
        // Chat command to create a new coinflip
        [ChatCommand("cf")]
        void cf(BasePlayer player, string command, string[] args)
        {
            if (args == null || args.Length < 1)
            {
                CuiElementContainer menu = generate_menu(player);
                CuiHelper.AddUi(player, menu);
                return;
            }

            if (args.Length < 2)
            {
                SendReply(player, "To open the menu: /cf");
                SendReply(player, "To create a coin flip : /cf <bet amount> <heads/tails>");
                return;
            }

            if (coinFlips.ContainsKey(player.UserIDString))
            {
                SendReply(player, "You can only have one active coin flip");
            }

            int side = 0;
            double amount = double.Parse(args[0]);
            string choice = args[1];
            
            if (choice.Substring(0, 1) == "h")
            {
                side = 1;
            }
            else if(choice.Substring(0, 1) == "t")
            {
                side = 2;
            }

            ActiveCoinFlip newFlip = new ActiveCoinFlip(Convert.ToInt32(amount), player, side);
            coinFlips.Add(player.UserIDString, newFlip);
            Economics.Call("Withdraw", player.UserIDString, amount);
            SendReply(player, "- $" + amount.ToString());
        }

        // Chat command to show all current coinflips
        [ChatCommand("show")]
        void show(BasePlayer player)
        {
            foreach (KeyValuePair<string, ActiveCoinFlip> kvp in coinFlips)
            {
                SendReply(player, kvp.Value.show());
            }
        }

        // Console command to remove all current coinflips
        [ConsoleCommand("remove")]
        void remove() { coinFlips.Clear(); }

        // Command to close the UI
        [ConsoleCommand("close")]
        void menu_remove(ConsoleSystem.Arg Args) 
        {
            BasePlayer player = BasePlayer.FindByID(Convert.ToUInt64(Args.Args[0]));
            CuiHelper.DestroyUi(player, "menu_panel"); 
        }

        // Command to perform a coin flip
        [ConsoleCommand("flip")]
        void flip(ConsoleSystem.Arg Args)
        {
            string flipper = Args.Args[0];
            double amount = coinFlips[flipper].bet;
            BasePlayer player = BasePlayer.FindByID(Convert.ToUInt64(Args.Args[1]));
            Economics.Call("Withdraw", player.UserIDString, amount);
            SendReply(player, "- $" + amount);
            coin_flip(coinFlips[flipper], player);
            coinFlips.Remove(flipper);

            CuiHelper.DestroyUi(player, "menu_panel");
            CuiElementContainer menu = generate_menu(player);
            CuiHelper.AddUi(player, menu);
        }
        #endregion

        #region UI
        private CuiElementContainer generate_menu(BasePlayer player)
        {
            var elements = new CuiElementContainer();

            var panel = elements.Add(new CuiPanel{ 
                Image = {Color = "0.1 0.1 0.1 1"},
                RectTransform = {AnchorMin = "0.195 0.167", AnchorMax = "0.781 0.861"},
                CursorEnabled = true
            }, "Hud", "menu_panel");

            var close_button = elements.Add(new CuiButton
            {
                Button = { Command = "close " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                RectTransform = { AnchorMin = "0.9 0.9", AnchorMax = ".98 .97" },
                Text = { Text = "close", FontSize = 18, Align = TextAnchor.MiddleCenter }
            }, panel);

            double xMin = .0125;
            double xMax = .2;
            double yMin = .7;
            double yMax = .88;
            int i = 0;
            foreach (KeyValuePair<string, ActiveCoinFlip> kvp in coinFlips)
            {
                if (i < 4)
                {
                    var join_cf = elements.Add(new CuiButton
                    {
                        Button = { Command = "flip " + kvp.Value.target.UserIDString + " " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                        RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString()},
                        Text = { Text = kvp.Value.target.IPlayer.Name + "\n" + kvp.Value.bet.ToString(), FontSize = 18, Align = TextAnchor.MiddleCenter }
                    }, panel);
                    xMin += .2;
                    xMax += .2;
                }
                else if (i < 8)
                {
                    if (i == 4)
                    {
                        xMin = .0125;
                        xMax = .2;
                        yMin = .5;
                        yMax = .68;
                    }
                    var join_cf = elements.Add(new CuiButton
                    {
                        Button = { Command = "flip " + kvp.Value.target.UserIDString + " " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                        RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString()},
                        Text = { Text = kvp.Value.target.IPlayer.Name + "\n" + kvp.Value.bet.ToString(), FontSize = 18, Align = TextAnchor.MiddleCenter }
                    }, panel);
                    xMin += .2;
                    xMax += .2;
                }
                else if (i < 12)
                {
                    if (i == 8)
                    {
                        xMin = .0125;
                        xMax = .2;
                        yMin = .3;
                        yMax = .48;
                    }
                    var join_cf = elements.Add(new CuiButton
                    {
                        Button = { Command = "flip " + kvp.Value.target.UserIDString + " " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                        RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString() },
                        Text = { Text = kvp.Value.target.IPlayer.Name + "\n" + kvp.Value.bet.ToString(), FontSize = 18, Align = TextAnchor.MiddleCenter }
                    }, panel);
                    xMin += .2;
                    xMax += .2;
                }
                else if (i < 16)
                {
                    if (i == 12)
                    {
                        xMin = .0125;
                        xMax = .2;
                        yMin = .1;
                        yMax = .28;
                    }
                    var join_cf = elements.Add(new CuiButton
                    {
                        Button = { Command = "flip " + kvp.Value.target.UserIDString + " " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                        RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString() },
                        Text = { Text = kvp.Value.target.IPlayer.Name + "\n" + kvp.Value.bet.ToString(), FontSize = 18, Align = TextAnchor.MiddleCenter }
                    }, panel);
                    xMin += .2;
                    xMax += .2;
                }
                i++;
            }
            while (i < 4)
            {
                var join_cf = elements.Add(new CuiButton
                {
                    Button = { Command = "close " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                    RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString()},
                    Text = { Text = "Empty", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, panel);
                xMin += .2;
                xMax += .2;
                i++;
            }

            while (i < 8)
            {
                if (i == 4)
                {
                    xMin = .0125;
                    xMax = .2;
                    yMin = .5;
                    yMax = .68;
                }

                var join_cf = elements.Add(new CuiButton
                {
                    Button = { Command = "close " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                    RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString()},
                    Text = { Text = "Empty", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, panel);
                xMin += .2;
                xMax += .2;
                i++;
            }

            while (i < 12)
            {
                if (i == 8)
                {
                    xMin = .0125;
                    xMax = .2;
                    yMin = .3;
                    yMax = .48;
                }

                var join_cf = elements.Add(new CuiButton
                {
                    Button = { Command = "close " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                    RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString() },
                    Text = { Text = "Empty", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, panel);
                xMin += .2;
                xMax += .2;
                i++;
            }

            while (i < 16)
            {
                if (i == 12)
                {
                    xMin = .0125;
                    xMax = .2;
                    yMin = .1;
                    yMax = .28;
                }

                var join_cf = elements.Add(new CuiButton
                {
                    Button = { Command = "close " + player.UserIDString, Color = "0.8 0.8 0.8 0.2" },
                    RectTransform = { AnchorMin = xMin.ToString() + " " + yMin.ToString(), AnchorMax = xMax.ToString() + " " + yMax.ToString() },
                    Text = { Text = "Empty", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, panel);
                xMin += .2;
                xMax += .2;
                i++;
            }
            return elements;
        }

        #endregion

        #region Coin Flip Controls

        private void coin_flip(ActiveCoinFlip creator, BasePlayer player)
        {
            double amount = creator.bet * 2;
            if (random.Next(1, 3) == creator.side)
            {
                SendReply(creator.target, "You won your coin flip!\n+ $" + amount.ToString());
                Economics.Call("Deposit", creator.target.UserIDString, amount);
                SendReply(player, "You lost your coin flip");
            }
            else
            {
                SendReply(player, "You won your coin flip!\n+ $" + amount.ToString());
                Economics.Call("Deposit", player.UserIDString, amount);
                SendReply(creator.target, "You lost your coin flip");
            }
        }

        #endregion
    }

}