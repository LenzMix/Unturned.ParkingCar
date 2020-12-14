using System;
using System.Collections.Generic;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned;
using System.Threading.Tasks;
using System.Threading;
using Rocket.API.Collections;

namespace SDParkingCar
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin Instance;
        public DatabaseManager Database;
        public SDParkingCar.Events.EventCars ECars;
        public bool isWorked = false;
        public override TranslationList DefaultTranslations
        {
            get
            {
                TranslationList translationList = new TranslationList();
                translationList.Add("marker", "< YOUR VEHICLE >");
                translationList.Add("newcar1", "[ТС System] Now It is your vehicle! You always can take it from garage");
                translationList.Add("newcar2", "[ТС System] Vehicle control - /car");
                translationList.Add("vehcoord", "[ТС System] Check vehicle position on GPS");
                translationList.Add("vehspawn", "[ТС System] Your vehicle {0} was spawned");
                translationList.Add("vehevac", "[ТС System] Your vehicle {0} was evacuated");
                translationList.Add("vehtp", "[ТС System] Your vehicle {0} was delivered");
                translationList.Add("usage1", "[ТС System] Use this: /car <function> [id]");
                translationList.Add("usage2", "[ТС System] Functions: claim / take / unlock / gps / list / evacuation / tp / unclaim");
                translationList.Add("usage3", "[ТС System] Teleportation cost ${0}");
                translationList.Add("usage4", "[ТС System] Evacuation cost ${0}");
                translationList.Add("errorarg", "[ТС System] Invalid argument");
                translationList.Add("errorcost", "[ТС System] You dont't have money for this action");
                translationList.Add("errornotincar", "[ТС System] You must be in car");
                translationList.Add("errornoactive", "[ТС System] You don't have active car");
                translationList.Add("erroralready", "[ТС System] Your car already spawned");
                translationList.Add("errornotclose", "[ТС System] This car not close or it's not your car");
                translationList.Add("errornotyour", "[ТС System] You don't have this car");
                translationList.Add("errorfraction", "[ТС System] It's not your car or It's fraction's car");
                translationList.Add("errorevacuation", "[ТС System] Your car not broken, we can't evacuation car");
                translationList.Add("successclaim", "[ТС System] Now it's your car! Use /car for control");
                translationList.Add("successunclaim", "[ТС System] Now it's not your car!");
                translationList.Add("successunlock", "[ТС System] You open your car!");
                translationList.Add("adminsuccess", "[ТС System] Admin command was success");
                translationList.Add("list1", "[ТС System] Your cars:");
                translationList.Add("list2", "ID: {0} | Name: {2} | Car ID: {1}");
                return translationList;
            }
        }

        protected override void Load()
        {
            Database = new DatabaseManager(this);
            ECars = new Events.EventCars(this);
            Instance = this;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            VehicleManager.onEnterVehicleRequested += OnVehEnter;
            VehicleManager.onExitVehicleRequested += OnVehExit;
            VehicleManager.onDamageVehicleRequested += OnVehDamage;
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("|                 Imperial Plugins Version                 |", System.ConsoleColor.Blue);
            Logger.Log("|                   SodaDevs: ParkingCar                   |", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("Version: " + Assembly.GetName().Version, System.ConsoleColor.Blue);
            OnStartServer();
        }

        public void StartCleaner ()
        {
            Thread.Sleep(10000);
            try
            {
                if (VehicleManager.vehicles.Count > 0)
                {
                    List<InteractableVehicle> cars = VehicleManager.vehicles;
                    isWorked = true;
                    if (cars.Count > 0 && Provider.clients.Count > 0)
                    {
                        foreach (InteractableVehicle car in cars)
                        {
                            uint vehid = car.instanceID;
                            bool isUsing = false;
                            if (Provider.clients.Count > 0)
                            {
                                foreach (SteamPlayer player in Provider.clients)
                                {
                                    UnturnedPlayer UP = UnturnedPlayer.FromSteamPlayer(player);
                                    PlayerComponent component = UP.GetComponent<PlayerComponent>();
                                    if (component.ActiveCar == vehid)
                                    {
                                        isUsing = true;
                                    }
                                }
                                if (!isUsing)
                                {
                                    VehicleManager.askVehicleDestroy(car);
                                }
                            }
                        }
                    }
                    else if (cars.Count > 0)
                    {
                        foreach (InteractableVehicle car in cars)
                        {
                            VehicleManager.askVehicleDestroy(car);
                        }
                    }
                }
            }
            catch
            {
                if (isWorked == false)
                    StartCleaner();
            }
        }

        public async void OnStartServer()
        {
            await Task.Run(() => StartCleaner()); 
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            foreach (SteamPlayer p in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(p);
                PlayerComponent component = player.GetComponent<PlayerComponent>();
                if (component.ActiveCar != 0)
                {
                    Events.EventCars.RemoveVehicleSaveActive(player);
                }
                Database.UpdateCars(player);
            }
        }

        public void OnVehEnter(Player player, InteractableVehicle vehicle, ref bool shouldAllow)
        {
            if (vehicle.isDead || vehicle.isDrowned || vehicle.isExploded || vehicle.health < Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp + 1)) || vehicle.isUnderwater)
            {
                VehicleManager.sendVehicleHealth(vehicle, Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp)));
                VehicleManager.sendVehicleBatteryCharge(vehicle, 0);
                VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp))
                });
                VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) 0
                });
            }
            UnturnedPlayer UP = UnturnedPlayer.FromPlayer(player);
            PlayerComponent component = player.GetComponent<PlayerComponent>();
            if (vehicle.lockedOwner == UP.CSteamID && vehicle.isLocked)
            {
                if (!SDParkingCar.Events.EventCars.IsSomeOne(vehicle.instanceID) && !SDParkingCar.Events.EventCars.IsBL(vehicle.id))
                {
                    SDParkingCar.Events.EventCars.RemoveVehicle(UP);
                    int newcarid = Database.AddCarBackID(UP, vehicle);
                    component.ActiveCar = vehicle.instanceID;
                    component.ActiveCarID = newcarid;
                    CarInfo NewCar = new CarInfo
                    {
                        id = newcarid,
                        IDcar = vehicle.id,
                        x = vehicle.transform.position.x,
                        y = vehicle.transform.position.y,
                        z = vehicle.transform.position.z,
                        rx = vehicle.transform.rotation.x,
                        ry = vehicle.transform.rotation.y,
                        rz = vehicle.transform.rotation.z,
                        rw = vehicle.transform.rotation.w,
                        Health = vehicle.health,
                        Gas = vehicle.fuel,
                        Battery = vehicle.batteryCharge,
                        _isTires = true,
                        _isActive = vehicle.instanceID,
                        name = vehicle.asset.name,
                    };
                    component.MyCars.Add(NewCar);
                    UnturnedChat.Say(UP, Plugin.Instance.Translate("successclaim", new object[0]), Color.yellow);
                }
            }
            
        }

        public void OnVehExit(Player player, InteractableVehicle vehicle, ref bool shouldAllow, ref Vector3 pendingLocation, ref float pendingYaw)
        {
            UnturnedPlayer UP = UnturnedPlayer.FromPlayer(player);
            PlayerComponent component = player.GetComponent<PlayerComponent>();
            if (vehicle.isDead || vehicle.isDrowned || vehicle.isExploded || vehicle.health < Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp + 1)) || vehicle.isUnderwater)
            {
                VehicleManager.sendVehicleHealth(vehicle, Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp)));
                VehicleManager.sendVehicleBatteryCharge(vehicle, 0);
                VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) Convert.ToUInt16((float)(Instance.Configuration.Instance.brokenhp))
                });
                VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) 0
                });
            }

        }

        public void OnVehDamage(Steamworks.CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (vehicle.health < Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp+1)))
            {
                shouldAllow = false;
                VehicleManager.sendVehicleHealth(vehicle, Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp)));
                VehicleManager.sendVehicleBatteryCharge(vehicle, 0);
                VehicleManager.instance.channel.send("tellVehicleHealth", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) Convert.ToUInt16(((float)vehicle.asset.healthMax / 100) * (float)(Instance.Configuration.Instance.brokenhp))
                });
                VehicleManager.instance.channel.send("tellVehicleBatteryCharge", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[2]
                {
                        (object) vehicle.instanceID,
                        (object) 0
                });
            }
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            PlayerComponent component = player.GetComponent<PlayerComponent>();
            if (component.ActiveCar != 0)
            {
                SDParkingCar.Events.EventCars.RemoveVehicleSaveActive(player);
            }
            Database.UpdateCars(player);    
        }

        public void OnPlayerConnected(UnturnedPlayer player)
        {
            if (Provider.clients.Count == 1)
            {
                StartCleaner();
            }
            PlayerComponent component = player.GetComponent<PlayerComponent>();
            Database.GetCars(player);
            foreach (CarInfo i in component.MyCars)
            {
                if (i._isActive != uint.MaxValue)
                {
                    if (component.ActiveCar == 0)
                    {
                        SDParkingCar.Events.EventCars.SpawnVehicle(player, i.id);
                    }
                    else
                        i._isActive = uint.MaxValue;
                }
            }
        }

        public class BarrInfo
        {
            public int barrid;
            public ushort health;
            public byte[] barrstate;
            public string owner;
            public string group;
            public float x;
            public float y;
            public float z;
            public float anglex;
            public float angley;
            public float anglez;
        }

        public class CarInfo
        {
            public int id;
            public ushort IDcar;
            public float x;
            public float y;
            public float z;
            public float rx;
            public float ry;
            public float rz;
            public float rw;
            public ushort Health;
            public ushort Gas;
            public ushort Battery;
            public bool _isTires;
            public uint _isActive;
            public string name;
        }

        public class PlayerComponent : UnturnedPlayerComponent
        {
            protected override void Load()
            {
                this.ActiveCar = 0;
                this.ActiveCarID = 0;
                this.MyCars = new List<CarInfo>();
            }

            public uint ActiveCar;
            public int ActiveCarID;
            public List<CarInfo> MyCars;
        }
    }
}
