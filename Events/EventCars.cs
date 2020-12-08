using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SDParkingCar.Events
{
    public class EventCars
    {
        private readonly Plugin _plugin;

        public EventCars(Plugin plugin)
        {
            _plugin = plugin;
        }
        public int AddCar(UnturnedPlayer player, InteractableVehicle mycar)
        {
            return Plugin.Instance.Database.AddCarBackID(player, mycar);
        }

        public static bool IsSomeOne(uint vehid)
        {
            bool NeedInfo = false;
            foreach (SteamPlayer playersteam in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(playersteam);
                Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                if (component.ActiveCar == vehid)
                {
                    NeedInfo = true;
                    break;
                }
            }
            return NeedInfo;
        }

        public static bool IsBL(ushort vehid)
        {
            bool NeedKnow = false;
            foreach (Config.blc i in Plugin.Instance.Configuration.Instance.blockedcars)
            {
                if (i.id == vehid)
                {
                    NeedKnow = true;
                    break;
                }
            }
            return NeedKnow;
        }

        public static void RemoveVehicle(UnturnedPlayer player)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            if (component.ActiveCar != 0 && component.ActiveCarID != 0)
            {
                InteractableVehicle MyCar = VehicleManager.getVehicle(component.ActiveCar);
                foreach (Plugin.CarInfo i in component.MyCars)
                {
                    if (i._isActive && i.IDcar == MyCar.id && component.ActiveCarID == i.id)
                    {
                        i.x = MyCar.transform.position.x;
                        i.y = MyCar.transform.position.y;
                        i.z = MyCar.transform.position.z;
                        i.rx = MyCar.transform.rotation.x;
                        i.ry = MyCar.transform.rotation.y;
                        i.rz = MyCar.transform.rotation.z;
                        i.rw = MyCar.transform.rotation.w;
                        i.Health = MyCar.health;
                        i.Battery = MyCar.batteryCharge;
                        i.Gas = MyCar.fuel;
                        i._isActive = false;
                        i.name = MyCar.asset.name;


                        BarricadeRegion vregion = BarricadeManager.findRegionFromVehicle(MyCar, 0);
                        List<Plugin.BarrInfo> list = new List<Plugin.BarrInfo>();
                        foreach (BarricadeDrop drop in vregion.drops)
                        {
                            BarricadeData data = vregion.findBarricadeByInstanceID(drop.instanceID);
                            BarricadeManager.tryGetInfo(drop.model, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region1, out BarricadeDrop drop1);
                            Plugin.BarrInfo NewBarr = new Plugin.BarrInfo
                            {
                                barrid = drop.asset.id,
                                barrstate = data.barricade.state,
                                health = data.barricade.health,
                                owner = Convert.ToString(data.owner),
                                group = Convert.ToString(data.group),
                                x = (float)data.point.x,
                                y = (float)data.point.y,
                                z = (float)data.point.z,
                                anglex = (float)data.angle_x,
                                angley = (float)data.angle_y,
                                anglez = (float)data.angle_z,
                            };
                            if (drop.interactable is InteractableStorage intstor)
                            {
                                intstor.items.clear();
                            };
                            if (drop.interactable is InteractableMannequin intman)
                            {
                                intman.clearClothes();
                                intman.clearVisuals();
                            };
                            BarricadeManager.destroyBarricade(region1, x, y, plant, index);
                            Plugin.Instance.Database.AddBarricades(i.id, list);
                        }


                        VehicleManager.askVehicleDestroy(MyCar);
                        VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, MyCar.instanceID);
                        component.ActiveCar = 0;
                        component.ActiveCarID = 0;
                        break;
                    }
                }
            }
        }

        public static void RemoveVehicleSaveActive(UnturnedPlayer player)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            if (component.ActiveCar != 0 && component.ActiveCarID != 0)
            {
                InteractableVehicle MyCar = VehicleManager.getVehicle(component.ActiveCar);
                foreach (Plugin.CarInfo i in component.MyCars)
                {
                    if (i._isActive && i.IDcar == MyCar.id && component.ActiveCarID == i.id)
                    {
                        i.x = MyCar.transform.position.x;
                        i.y = MyCar.transform.position.y;
                        i.z = MyCar.transform.position.z;
                        i.rx = MyCar.transform.rotation.x;
                        i.ry = MyCar.transform.rotation.y;
                        i.rz = MyCar.transform.rotation.z;
                        i.rw = MyCar.transform.rotation.w;
                        i.Health = MyCar.health;
                        i.Battery = MyCar.batteryCharge;
                        i.Gas = MyCar.fuel;
                        i._isActive = true;
                        i.name = MyCar.asset.name;


                        BarricadeRegion vregion = BarricadeManager.findRegionFromVehicle(MyCar, 0);
                        List<Plugin.BarrInfo> list = new List<Plugin.BarrInfo>();
                        foreach (BarricadeDrop drop in vregion.drops)
                        {
                            BarricadeData data = vregion.findBarricadeByInstanceID(drop.instanceID);
                            BarricadeManager.tryGetInfo(drop.model, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region1, out BarricadeDrop drop1);
                            Plugin.BarrInfo NewBarr = new Plugin.BarrInfo
                            {
                                barrid = drop.asset.id,
                                barrstate = data.barricade.state,
                                health = data.barricade.health,
                                owner = Convert.ToString(data.owner),
                                group = Convert.ToString(data.group),
                                x = (float)data.point.x,
                                y = (float)data.point.y,
                                z = (float)data.point.z,
                                anglex = (float)data.angle_x,
                                angley = (float)data.angle_y,
                                anglez = (float)data.angle_z,
                            };
                            if (drop.interactable is InteractableStorage intstor)
                            {
                                intstor.items.clear();
                            };
                            if (drop.interactable is InteractableMannequin intman)
                            {
                                intman.clearClothes();
                                intman.clearVisuals();
                            };
                            BarricadeManager.destroyBarricade(region1, x, y, plant, index);
                            Plugin.Instance.Database.AddBarricades(i.id, list);
                        }


                        VehicleManager.askVehicleDestroy(MyCar);
                        VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, MyCar.instanceID);
                        component.ActiveCar = 0;
                        component.ActiveCarID = 0;
                        break;
                    }
                }
            }
        }

        public static void SpawnVehicleBad(UnturnedPlayer player, int id)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            foreach (Plugin.CarInfo i in component.MyCars)
            {
                if (i.id == id)
                {
                    Vector3 carpos = new Vector3(i.x, i.y, i.z);
                    Vector3 mehpos = carpos;
                    if (Plugin.Instance.Configuration.Instance.mechanic.Count > 0)
                    {
                        foreach (Config.Coordinate i2 in Plugin.Instance.Configuration.Instance.mechanic)
                        {
                            Vector3 newmehpos = new Vector3(i2.x, i2.y, i2.z);
                            if (Vector3.Distance(carpos, mehpos) > Vector3.Distance(carpos, newmehpos) || carpos == mehpos)
                            {
                                mehpos = newmehpos;
                            }
                        }
                    }
                    InteractableVehicle MyCar = VehicleManager.spawnLockedVehicleForPlayerV2(i.IDcar, mehpos, new Quaternion(i.rx, i.ry, i.rz, i.rw), player.Player);
                    i.x = mehpos.x;
                    i.y = mehpos.y;
                    i.z = mehpos.z;
                    VehicleManager.sendVehicleHealth(MyCar, i.Health);
                    VehicleManager.sendVehicleFuel(MyCar, i.Gas);
                    VehicleManager.sendVehicleBatteryCharge(MyCar, i.Battery);
                    VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Health
                    });
                    VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Gas
                    });
                    VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Battery
                    });
                    i._isActive = true;
                    UnturnedChat.Say(player, Plugin.Instance.Translate("vehevac", new object[] { Convert.ToString(i.name) }), Color.yellow);
                    UnturnedChat.Say(player, Plugin.Instance.Translate("vehcoord", new object[0]), Color.yellow);
                    player.Player.quests.replicateSetMarker(true, MyCar.transform.position, Plugin.Instance.Translate("marker", new object[0]));
                    component.ActiveCar = MyCar.instanceID;
                    component.ActiveCarID = i.id;


                    BarricadeRegion vregion = BarricadeManager.findRegionFromVehicle(MyCar, 0);
                    List<Plugin.BarrInfo> list = Plugin.Instance.Database.GetBarricades(i.id);
                    foreach (Plugin.BarrInfo barr in list)
                    {
                        Barricade newBarr = new Barricade((ushort)barr.barrid);
                        Transform tranBarr = BarricadeManager.dropPlantedBarricade(vregion.parent, newBarr, new Vector3(barr.x, barr.y, barr.z), BarricadeManager.getRotation(newBarr.asset, barr.anglex, barr.angley, barr.anglez), Convert.ToUInt64(barr.owner), Convert.ToUInt64(barr.group));
                        BarricadeManager.tryGetInfo(tranBarr, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop);
                        BarricadeData data = region.findBarricadeByInstanceID(drop.instanceID);
                        data.barricade.health = barr.health;
                        drop.interactable.updateState(data.barricade.asset, barr.barrstate);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.owner)).CopyTo(data.barricade.state, 0);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.group)).CopyTo(data.barricade.state, 8);
                    }
                }
            }
        }


        public static void SpawnVehicle(UnturnedPlayer player, int id)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            foreach (Plugin.CarInfo i in component.MyCars)
            {
                if (i.id == id)
                {
                    InteractableVehicle MyCar = VehicleManager.spawnLockedVehicleForPlayerV2(i.IDcar, new Vector3(i.x, i.y + 3, i.z), new Quaternion(i.rx, i.ry, i.rz, i.rw), player.Player);
                    VehicleManager.sendVehicleHealth(MyCar, i.Health);
                    VehicleManager.sendVehicleFuel(MyCar, i.Gas);
                    VehicleManager.sendVehicleBatteryCharge(MyCar, i.Battery);
                    VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Health
                    });
                    VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Gas
                    });
                    VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Battery
                    });
                    i._isActive = true;
                    UnturnedChat.Say(player, Plugin.Instance.Translate("vehspawn", new object[] { Convert.ToString(i.name) }), Color.yellow);
                    UnturnedChat.Say(player, Plugin.Instance.Translate("vehcoord", new object[0]), Color.yellow);
                    player.Player.quests.replicateSetMarker(true, MyCar.transform.position, Plugin.Instance.Translate("marker", new object[0]));
                    component.ActiveCar = MyCar.instanceID;
                    component.ActiveCarID = i.id;


                    BarricadeRegion vregion = BarricadeManager.findRegionFromVehicle(MyCar, 0);
                    List<Plugin.BarrInfo> list = Plugin.Instance.Database.GetBarricades(i.id);
                    foreach (Plugin.BarrInfo barr in list)
                    {
                        Barricade newBarr = new Barricade((ushort)barr.barrid);
                        Transform tranBarr = BarricadeManager.dropPlantedBarricade(vregion.parent, newBarr, new Vector3(barr.x, barr.y, barr.z), BarricadeManager.getRotation(newBarr.asset, barr.anglex, barr.angley, barr.anglez), Convert.ToUInt64(barr.owner), Convert.ToUInt64(barr.group));
                        BarricadeManager.tryGetInfo(tranBarr, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop);
                        BarricadeData data = region.findBarricadeByInstanceID(drop.instanceID);
                        data.barricade.health = barr.health;
                        drop.interactable.updateState(data.barricade.asset, barr.barrstate);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.owner)).CopyTo(data.barricade.state, 0);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.group)).CopyTo(data.barricade.state, 8);
                    }
                }
            }
        }


        public static void SpawnVehicleTP(UnturnedPlayer player, int id)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            foreach (Plugin.CarInfo i in component.MyCars)
            {
                if (i.id == id)
                {
                    InteractableVehicle MyCar = VehicleManager.spawnLockedVehicleForPlayerV2(i.IDcar, new Vector3(player.Position.x, player.Position.y + 3, player.Position.z), new Quaternion(i.rx, i.ry, i.rz, i.rw), player.Player);
                    VehicleManager.sendVehicleHealth(MyCar, i.Health);
                    VehicleManager.sendVehicleFuel(MyCar, i.Gas);
                    VehicleManager.sendVehicleBatteryCharge(MyCar, i.Battery);
                    VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Health
                    });
                    VehicleManager.instance.channel.send("tellVehicleFuel", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Gas
                    });
                    VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                    {
                        (object) MyCar.instanceID,
                        (object) i.Battery
                    });
                    i.x = player.Position.x;
                    i.y = player.Position.y;
                    i.z = player.Position.z;
                    i._isActive = true;
                    UnturnedChat.Say(player, Plugin.Instance.Translate("vehspawn", new object[] { Convert.ToString(i.name) }), Color.yellow);
                    component.ActiveCar = MyCar.instanceID;
                    component.ActiveCarID = i.id;


                    BarricadeRegion vregion = BarricadeManager.findRegionFromVehicle(MyCar, 0);
                    List<Plugin.BarrInfo> list = Plugin.Instance.Database.GetBarricades(i.id);
                    foreach (Plugin.BarrInfo barr in list)
                    {
                        Barricade newBarr = new Barricade((ushort)barr.barrid);
                        Transform tranBarr = BarricadeManager.dropPlantedBarricade(vregion.parent, newBarr, new Vector3(barr.x, barr.y, barr.z), BarricadeManager.getRotation(newBarr.asset, barr.anglex, barr.angley, barr.anglez), Convert.ToUInt64(barr.owner), Convert.ToUInt64(barr.group));
                        BarricadeManager.tryGetInfo(tranBarr, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop);
                        BarricadeData data = region.findBarricadeByInstanceID(drop.instanceID);
                        data.barricade.health = barr.health;
                        drop.interactable.updateState(data.barricade.asset, barr.barrstate);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.owner)).CopyTo(data.barricade.state, 0);
                        BitConverter.GetBytes(Convert.ToUInt64(barr.group)).CopyTo(data.barricade.state, 8);
                    }
                }
            }
        }
    }
}
