using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SDParkingCar
{
    class CommandAdminGarage : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Name
        {
            get
            {
                return "admincar";
            }
        }

        public string Help
        {
            get
            {
                return "";
            }
        }

        public string Syntax
        {
            get
            {
                return "Usage: /admincar <addmeh/clearmeh> [id]";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>
                {
                    "admincar"
                };
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>
                {
                    "command.admingarage"
                };
            }
        }


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            if (command.Length > 0)
            {
                if (command[0] == "addmeh")
                {
                    Plugin.Instance.Configuration.Instance.mechanic.Add(new Config.Coordinate() { x = player.Position.x, y = player.Position.y + 3, z = player.Position.z });
                    UnturnedChat.Say(player, Plugin.Instance.Translate("adminsuccess", new object[0]), Color.yellow);
                    Plugin.Instance.Configuration.Save();
                }
                else if (command[0] == "clearmeh")
                {
                    Plugin.Instance.Configuration.Instance.mechanic.Clear();
                    UnturnedChat.Say(player, Plugin.Instance.Translate("adminsuccess", new object[0]), Color.yellow);
                    Plugin.Instance.Configuration.Save();
                }

            }
        }
    }
}
