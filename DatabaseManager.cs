using MySql.Data.MySqlClient;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SDParkingCar
{
    public class DatabaseManager
    {
        private readonly Plugin _plugin;

        public DatabaseManager(Plugin plugin)
        {
            _plugin = plugin;
            SQLChecker();
        }

        private MySqlConnection createConnection()
        {
            MySqlConnection connection = null;
            connection = new MySqlConnection($"SERVER=" + _plugin.Configuration.Instance.mysqlip + ";DATABASE=" + _plugin.Configuration.Instance.mysqldb + ";UID=" + _plugin.Configuration.Instance.mysqlusr + ";PASSWORD=" + _plugin.Configuration.Instance.mysqlpass + ";PORT=" + _plugin.Configuration.Instance.mysqlport + ";");

            return connection;
        }

        private void SQLChecker()
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "show tables like '" + _plugin.Configuration.Instance.mysqltable + "'";
            connection.Open();
            object test1 = command.ExecuteScalar();

            if (test1 == null)
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS `" + _plugin.Configuration.Instance.mysqltable + "` (`id` int(11) NOT NULL AUTO_INCREMENT,`steamid` text, `carid` int(11) DEFAULT NULL,  `x` text,  `y` text,  `z` text,  `health` int(11) DEFAULT NULL, `gas` int(11) DEFAULT NULL,  `isTires` int(11) DEFAULT NULL,  `isActive` int(11) DEFAULT NULL,  `rx` text,  `ry` text,  `rz` text,  `rw` text,  `battery` int(11) DEFAULT NULL,  `name` text,  PRIMARY KEY(`id`));";
                command.ExecuteNonQuery();
            }
            command.CommandText = "show tables like '" + _plugin.Configuration.Instance.mysqltableitems + "'";
            object test2 = command.ExecuteScalar();

            if (test2 == null)
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS `" + _plugin.Configuration.Instance.mysqltableitems + "` (`id` int(11) NOT NULL AUTO_INCREMENT,`carid` int(11) DEFAULT NULL,  `x` text,  `y` text,  `z` text,  `anglex` text,  `angley` text,  `anglez` text,  `barrid` int(11) DEFAULT NULL,  `barrstate` longtext DEFAULT NULL,  `barrhealth` int DEFAULT NULL,  `owner` text DEFAULT NULL,  `group` text DEFAULT NULL,  PRIMARY KEY(`id`));";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        public List<Plugin.BarrInfo> GetBarricades(int car)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "SELECT * FROM " + Plugin.Instance.Configuration.Instance.mysqltableitems + " WHERE carid = '" + car.ToString() + "';";
            MySqlDataReader result = command.ExecuteReader();
            List<Plugin.BarrInfo> list = new List<Plugin.BarrInfo>();
            if (result.HasRows)
            {
                while (result.Read())
                {
                    Plugin.BarrInfo NewBarr = new Plugin.BarrInfo
                    {
                        barrid = result.GetInt32("barrid"),
                        barrstate = Convert.FromBase64String(result.GetString("barrstate")),
                        health = result.GetUInt16("barrhealth"),
                        owner = result.GetString("owner"),
                        group = result.GetString("group"),
                        x = (float)Convert.ToDecimal(result.GetString("x")),
                        y = (float)Convert.ToDecimal(result.GetString("y")),
                        z = (float)Convert.ToDecimal(result.GetString("z")),
                        anglex = (float)Convert.ToDecimal(result.GetString("anglex")),
                        angley = (float)Convert.ToDecimal(result.GetString("angley")),
                        anglez = (float)Convert.ToDecimal(result.GetString("anglez")),
                    };
                    Logger.Log("SQL - Barr getting (" + NewBarr.barrid.ToString() + ")", System.ConsoleColor.Blue);
                }
            }
            result.Close();
            result.Dispose();
            connection.Close();
            return list;
        }

        public void AddBarricades(int car, List<Plugin.BarrInfo> list)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "DELETE FROM " + Plugin.Instance.Configuration.Instance.mysqltableitems + " WHERE carid = '" + car.ToString() + "';";
            command.ExecuteNonQuery();
            foreach (Plugin.BarrInfo barr in list)
            {
                command.CommandText = "INSERT INTO " + Plugin.Instance.Configuration.Instance.mysqltableitems + " (`carid`,`x`,`y`,`z`,`anglex`,`angley`,`anglez`,`barrhealth`,`barrid`,`barrstate`,`owner`,`group`) VALUES(" + car.ToString() + ", '" + barr.x.ToString() + ", '" + barr.y.ToString() + ", '" + barr.z.ToString() + ", '" + barr.anglex.ToString() + ", '" + barr.angley.ToString() + ", '" + barr.anglez.ToString() + ", '" + barr.health.ToString() + ", '" + barr.barrid.ToString() + ", '" + Convert.ToBase64String(barr.barrstate) + ", '" + barr.owner.ToString() + ", '" + barr.group.ToString() + "')";
                command.ExecuteNonQuery();
            }
            connection.Close();
            Logger.Log("SQL - BARRs ADD)", System.ConsoleColor.Blue);
        }

        public void AddCar(UnturnedPlayer player, InteractableVehicle vehicle)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            VehicleAsset v = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicle.id);
            command.CommandText = "INSERT INTO `"+ Plugin.Instance.Configuration.Instance.mysqltable + "` (`steamid`, `carid`,`x`,`y`,`z`,`health`,`gas`,`isActive`,`rx`,`ry`,`rz`,`rw`,`battery`,`name`) VALUES('" + player.CSteamID.ToString() + "', '" + vehicle.id.ToString() + "', '" + vehicle.transform.position.x.ToString() + "', '" + vehicle.transform.position.y.ToString() + "', '" + vehicle.transform.position.z.ToString() + "', '" + vehicle.health.ToString() + "', '" + vehicle.fuel.ToString() + "', '1', '" + vehicle.transform.rotation.x.ToString() + "', '" + vehicle.transform.rotation.y.ToString() + "', '" + vehicle.transform.rotation.z.ToString() + "', '" + vehicle.transform.rotation.w.ToString() + "', '" + vehicle.batteryCharge.ToString() + "','" + v.name + "')";
            command.ExecuteNonQuery();
            connection.Close();
            Logger.Log("SQL - Car added (" + player.CharacterName + ")", System.ConsoleColor.Blue);
        }

        public int AddCarBackID(UnturnedPlayer player, InteractableVehicle vehicle)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "INSERT INTO `"+ Plugin.Instance.Configuration.Instance.mysqltable + "` (`steamid`, `carid`,`x`,`y`,`z`,`health`,`gas`,`isActive`,`rx`,`ry`,`rz`,`rw`,`battery`,`name`) VALUES('" + player.CSteamID.ToString() + "', '" + vehicle.id.ToString() + "', '" + vehicle.transform.position.x.ToString() + "', '" + vehicle.transform.position.y.ToString() + "', '" + vehicle.transform.position.z.ToString() + "', '" + vehicle.health.ToString() + "', '" + vehicle.fuel.ToString() + "', '1', '" + vehicle.transform.rotation.x.ToString() + "', '" + vehicle.transform.rotation.y.ToString() + "', '" + vehicle.transform.rotation.z.ToString() + "', '" + vehicle.transform.rotation.w.ToString() + "', '" + vehicle.batteryCharge.ToString() + "','"+vehicle.asset.name+"')";
            command.ExecuteNonQuery();
            command.CommandText = "SELECT * FROM " + Plugin.Instance.Configuration.Instance.mysqltable + " WHERE steamid = '" + Convert.ToString(player.CSteamID.m_SteamID) + "' AND carid = '" + Convert.ToString(vehicle.id)+"';";
            MySqlDataReader result = command.ExecuteReader();
            int res = 0;
            if (result.HasRows)
            {
                while (result.Read())
                {
                    res = result.GetInt32("id");
                    Logger.Log("SQL - Car added (" + player.CharacterName + ")", System.ConsoleColor.Blue);
                }
            }
            result.Close();
            result.Dispose();
            connection.Close();
            return res;
        }

        public void RemoveCar(UnturnedPlayer player, int id)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "DELETE FROM " + Plugin.Instance.Configuration.Instance.mysqltable + " WHERE id = '"+id.ToString()+"';";
            command.ExecuteNonQuery();
            connection.Close();
            Logger.Log("SQL - Car deleted (" + id.ToString() + ")", System.ConsoleColor.Blue);
        }

        public void GetCars(UnturnedPlayer player)
        {
            MySqlConnection connection = createConnection();
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "SELECT * FROM " + Plugin.Instance.Configuration.Instance.mysqltable + " WHERE steamid = '" + Convert.ToString(player.CSteamID.m_SteamID) + "';";
            MySqlDataReader result = command.ExecuteReader();
            if (result.HasRows)
            {
                while (result.Read())
                {
                    bool IsActive = false;
                    if (result.GetInt32("IsActive") == 1)
                    {
                        IsActive = true;
                    }
                    Plugin.CarInfo NewCar = new Plugin.CarInfo
                    {
                        id = result.GetInt32("id"),
                        IDcar = result.GetUInt16("carid"),
                        x = (float)Convert.ToDecimal(result.GetString("x")),
                        y = (float)Convert.ToDecimal(result.GetString("y")),
                        z = (float)Convert.ToDecimal(result.GetString("z")),
                        rx = (float)Convert.ToDecimal(result.GetString("rx")),
                        ry = (float)Convert.ToDecimal(result.GetString("ry")),
                        rz = (float)Convert.ToDecimal(result.GetString("rz")),
                        rw = (float)Convert.ToDecimal(result.GetString("rw")),
                        Health = result.GetUInt16("health"),
                        Gas = result.GetUInt16("gas"),
                        Battery = result.GetUInt16("battery"),
                        name = result.GetString("name"),
                        _isTires = false,
                        _isActive = IsActive,
                    };
                    component.MyCars.Add(NewCar);
                    Logger.Log("SQL - Car getting (" + player.CharacterName + ")", System.ConsoleColor.Blue);
                }
            }
            result.Close();
            result.Dispose();
            connection.Close();
        }

        public void UpdateCars(UnturnedPlayer player)
        {
            MySqlConnection connection = createConnection();
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            foreach (Plugin.CarInfo i in component.MyCars)
            {
                string isAct = "0";
                if (i._isActive)
                {
                    isAct = "1";
                }
                command.CommandText = "UPDATE " + Plugin.Instance.Configuration.Instance.mysqltable + " SET carid = '" + Convert.ToString(i.IDcar) + "', x = '" + Convert.ToString(i.x) + "', y = '" + Convert.ToString(i.y) + "', z = '" + Convert.ToString(i.z) + "', rx = '" + Convert.ToString(i.rx) + "', ry = '" + Convert.ToString(i.ry) + "', rz = '" + Convert.ToString(i.rz) + "', rw = '" + Convert.ToString(i.rw) + "', health = '" + Convert.ToString(i.Health) + "', gas = '" + Convert.ToString(i.Gas) + "', battery = '" + Convert.ToString(i.Battery) + "', isActive = '" + isAct + "'  WHERE id = '" + Convert.ToString(i.id) + "';";
                command.ExecuteNonQuery();
                Logger.Log("SQL - Car update ("+player.CharacterName+")", System.ConsoleColor.Blue);
            }
            connection.Close();
        }
    }
}