using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using fr34kyn01535.Uconomy;

namespace SDParkingCar
{
    public class CommandGarage : IRocketCommand
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
                return "car".ToLower().ToUpper();
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
                return "Usage: /car <take/unlock/gps/claim/list/evacuation/tp> [id]";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>
                {
                    "car"
                };
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>
                {
                    "command.car"
                };
            }
        }


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            if (command.Length > 0)
            {
                if (command[0] == "take")
                {
                    if (command.Length == 2)
                    {
                        bool IsHaveCar = false;
                        foreach (Plugin.CarInfo i in component.MyCars)
                        {
                            if (i.id == Convert.ToInt32(command[1]))
                            {
                                if (i._isActive == false)
                                {
                                    IsHaveCar = true;
                                    Events.EventCars.RemoveVehicle(player);
                                    if (i.Health >= 20)
                                    {
                                        Events.EventCars.SpawnVehicle(player, Convert.ToInt32(command[1]));
                                    }
                                    else
                                    {
                                        Events.EventCars.SpawnVehicleBad(player, Convert.ToInt32(command[1]));

                                    }
                                    break;
                                }
                                else
                                {
                                    UnturnedChat.Say(player, Plugin.Instance.Translate("erroralready", new object[0]), Color.yellow);
                                }
                            }
                        }
                        if (IsHaveCar == false)
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornotyour", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorarg", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "unlock")
                {
                    if (command.Length == 1)
                    {
                        if (component.ActiveCar != 0)
                        {
                            InteractableVehicle vehicle = VehicleManager.getVehicle(component.ActiveCar);
                            VehicleManager.unlockVehicle(vehicle, player.Player);
                            UnturnedChat.Say(player, Plugin.Instance.Translate("successunlock", new object[0]), Color.yellow);
                        }
                        else
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornoactive", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorarg", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "tp")
                {
                    if (command.Length == 2)
                    {
                        bool IsHaveCar = false;
                        foreach (Plugin.CarInfo i in component.MyCars)
                        {
                            if (i.id == Convert.ToInt32(command[1]))
                            {
                                if (Plugin.Instance.Configuration.Instance.isUconomy == false)
                                {
                                    if (player.Experience >= Plugin.Instance.Configuration.Instance.costtp)
                                    {
                                        if (i._isActive == false)
                                        {
                                            player.Experience = player.Experience - (uint)Plugin.Instance.Configuration.Instance.costtp;
                                            IsHaveCar = true;
                                            Events.EventCars.RemoveVehicle(player);
                                            Events.EventCars.SpawnVehicleTP(player, Convert.ToInt32(command[1]));
                                        }
                                        else
                                        {
                                            Events.EventCars.RemoveVehicle(player);
                                            player.Experience = player.Experience - (uint)Plugin.Instance.Configuration.Instance.costtp;
                                            IsHaveCar = true;
                                            Events.EventCars.RemoveVehicle(player);
                                            Events.EventCars.SpawnVehicleTP(player, Convert.ToInt32(command[1]));
                                        }
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorcost", new object[0]), Color.yellow);
                                    }
                                }
                                else
                                {
                                    if (Uconomy.Instance.Database.GetBalance(player.CSteamID.m_SteamID.ToString()) >= (decimal)Plugin.Instance.Configuration.Instance.costtp)
                                    {
                                        if (i._isActive == false)
                                        {
                                            Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.m_SteamID.ToString(), -(decimal)Plugin.Instance.Configuration.Instance.costtp);
                                            IsHaveCar = true;
                                            Events.EventCars.RemoveVehicle(player);
                                            Events.EventCars.SpawnVehicleTP(player, Convert.ToInt32(command[1]));
                                        }
                                        else
                                        {
                                            Events.EventCars.RemoveVehicle(player);
                                            Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.m_SteamID.ToString(), -(decimal)Plugin.Instance.Configuration.Instance.costtp);
                                            IsHaveCar = true;
                                            Events.EventCars.RemoveVehicle(player);
                                            Events.EventCars.SpawnVehicleTP(player, Convert.ToInt32(command[1]));
                                        }
                                    }
                                    else
                                    {
                                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorcost", new object[0]), Color.yellow);
                                    }
                                }
                            }
                        }
                        if (IsHaveCar == false)
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornotyour", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorarg", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "evacuation")
                {
                    if (command.Length == 1)
                    {
                        if (component.ActiveCar != 0)
                        {
                            InteractableVehicle vehicle = VehicleManager.getVehicle(component.ActiveCar);
                            if (Plugin.Instance.Configuration.Instance.isUconomy == true)
                            {
                                if (Uconomy.Instance.Database.GetBalance(player.CSteamID.m_SteamID.ToString()) >= Plugin.Instance.Configuration.Instance.costfix)
                                {
                                    Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.m_SteamID.ToString(), -(decimal)Plugin.Instance.Configuration.Instance.costfix);
                                    int id = component.ActiveCarID;
                                    Events.EventCars.RemoveVehicle(player);
                                    Events.EventCars.SpawnVehicleBad(player, id);
                                    VehicleManager.sendVehicleFuel(vehicle, vehicle.asset.fuelMax);
                                    VehicleManager.sendVehicleHealth(vehicle, vehicle.asset.healthMax);
                                    VehicleManager.sendVehicleBatteryCharge(vehicle, 10000);
                                    VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) vehicle.asset.healthMax
                                    });
                                    VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) vehicle.asset.fuelMax
                                    });
                                    VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) 10000
                                    });
                                    vehicle.updateVehicle();
                                }
                                else
                                {
                                    UnturnedChat.Say(player, Plugin.Instance.Translate("errorcost", new object[0]), Color.yellow);
                                }
                            }
                            else
                            {
                                if (player.Experience >= Plugin.Instance.Configuration.Instance.costfix)
                                {
                                    player.Experience = player.Experience - (uint)Plugin.Instance.Configuration.Instance.costfix;
                                    int id = component.ActiveCarID;
                                    Events.EventCars.RemoveVehicle(player);
                                    Events.EventCars.SpawnVehicleBad(player, id);
                                    VehicleManager.sendVehicleFuel(vehicle, vehicle.asset.fuelMax);
                                    VehicleManager.sendVehicleHealth(vehicle, vehicle.asset.healthMax);
                                    VehicleManager.sendVehicleBatteryCharge(vehicle, 10000);
                                    VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) vehicle.asset.healthMax
                                    });
                                    VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) vehicle.asset.fuelMax
                                    });
                                    VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                                    {
                                    (object) vehicle.instanceID,
                                    (object) 10000
                                    });
                                    vehicle.updateVehicle();
                                }
                                else
                                {
                                    UnturnedChat.Say(player, Plugin.Instance.Translate("errorcost", new object[0]), Color.yellow);
                                }
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornoactive", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errorarg", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "gps")
                {
                    if (component.ActiveCar != 0)
                    {
                        InteractableVehicle MyCar = VehicleManager.getVehicle(component.ActiveCar);
                        UnturnedChat.Say(player, Plugin.Instance.Translate("vehcoord", new object[0]), Color.yellow);
                        player.Player.quests.replicateSetMarker(true, MyCar.transform.position, Plugin.Instance.Translate("marker", new object[0]));
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errornoactive", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "list")
                {
                    UnturnedChat.Say(player, Plugin.Instance.Translate("list1", new object[0]), Color.yellow);
                    foreach (Plugin.CarInfo i in component.MyCars)
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("list2", new object[] { Convert.ToString(i.id), Convert.ToString(i.IDcar), Convert.ToString(i.name) }), Color.yellow);
                    }
                }
                else if (command[0] == "claim")
                {

                    if (player.IsInVehicle)
                    {
                        InteractableVehicle mycar = player.CurrentVehicle;
                        if (mycar.lockedOwner == player.CSteamID && mycar.isLocked)
                        {
                            if (!Events.EventCars.IsSomeOne(mycar.instanceID) && !Events.EventCars.IsBL(mycar.id))
                            {
                                Events.EventCars.RemoveVehicle(player);
                                int newcarid = Plugin.Instance.Database.AddCarBackID(player, mycar);
                                component.ActiveCar = mycar.instanceID;
                                component.ActiveCarID = newcarid;
                                Plugin.CarInfo NewCar = new Plugin.CarInfo
                                {
                                    id = newcarid,
                                    IDcar = mycar.id,
                                    x = mycar.transform.position.x,
                                    y = mycar.transform.position.y,
                                    z = mycar.transform.position.z,
                                    rx = mycar.transform.rotation.x,
                                    ry = mycar.transform.rotation.y,
                                    rz = mycar.transform.rotation.z,
                                    rw = mycar.transform.rotation.w,
                                    Health = mycar.health,
                                    Gas = mycar.fuel,
                                    Battery = mycar.batteryCharge,
                                    _isTires = true,
                                    _isActive = true,
                                };
                                component.MyCars.Add(NewCar);
                                UnturnedChat.Say(player, Plugin.Instance.Translate("successclaim", new object[0]), Color.yellow);
                            }
                            else
                            {
                                UnturnedChat.Say(player, Plugin.Instance.Translate("errorfraction", new object[0]), Color.yellow);
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornotclose", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errornotincar", new object[0]), Color.yellow);
                    }
                }
                else if (command[0] == "unclaim")
                {
                    if (player.IsInVehicle)
                    {
                        InteractableVehicle mycar = player.CurrentVehicle;
                        if (mycar.lockedOwner == player.CSteamID && mycar.isLocked)
                        {
                            if (mycar.instanceID == component.ActiveCar)
                            {
                                foreach (Plugin.CarInfo i in component.MyCars)
                                {
                                    if (component.ActiveCarID == i.id)
                                        component.MyCars.Remove(i);
                                }
                                Plugin.Instance.Database.RemoveCar(player, component.ActiveCarID);
                                component.ActiveCar = 0;
                                component.ActiveCarID = 0;
                                UnturnedChat.Say(player, Plugin.Instance.Translate("successunclaim", new object[0]), Color.yellow);
                            }
                            else
                            {
                                UnturnedChat.Say(player, Plugin.Instance.Translate("errorfraction", new object[0]), Color.yellow);
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(player, Plugin.Instance.Translate("errornotclose", new object[0]), Color.yellow);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.Instance.Translate("errornotincar", new object[0]), Color.yellow);
                    }
                }
                else
                {
                    UnturnedChat.Say(player, Plugin.Instance.Translate("usage1", new object[0]), Color.yellow);
                    UnturnedChat.Say(player, Plugin.Instance.Translate("usage2", new object[0]), Color.yellow);
                    UnturnedChat.Say(player, Plugin.Instance.Translate("usage3", new object[] { Convert.ToString(Plugin.Instance.Configuration.Instance.costtp) }), Color.yellow);
                    UnturnedChat.Say(player, Plugin.Instance.Translate("usage4", new object[] { Convert.ToString(Plugin.Instance.Configuration.Instance.costfix) }), Color.yellow);
                }
            }
            else
            {
                UnturnedChat.Say(player, Plugin.Instance.Translate("usage1", new object[0]), Color.yellow);
                UnturnedChat.Say(player, Plugin.Instance.Translate("usage2", new object[0]), Color.yellow);
                UnturnedChat.Say(player, Plugin.Instance.Translate("usage3", new object[] { Convert.ToString(Plugin.Instance.Configuration.Instance.costtp) }), Color.yellow);
                UnturnedChat.Say(player, Plugin.Instance.Translate("usage4", new object[] { Convert.ToString(Plugin.Instance.Configuration.Instance.costfix) }), Color.yellow);
            }
        }
    }
}
