using System;
using System.Collections.Generic;
using Rocket.API;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SDParkingCar
{
    public class Config : IRocketPluginConfiguration, IDefaultable
    {
        public void LoadDefaults()
        {
            this.brokenhp = 10;
            this.isUconomy = false;
            this.costfix = 1000;
            this.costtp = 1000;
            this.blockedcars = new List<blc>()
            {
                new blc
                {
                    id = 23
                }
            };
            this.mechanic = new List<Coordinate>();
            this.mysqlip = "IP";
            this.mysqlusr = "USER";
            this.mysqlpass = "PASS";
            this.mysqlport = "PORT";
            this.mysqldb = "DATABASE";
            this.mysqltable = "TABLE";
            this.mysqltableitems = "TABLEITEMS";
        }

        public bool isUconomy;
        public int brokenhp;
        public int costtp;
        public int costfix;
        public List<blc> blockedcars;
        public string mysqlip;
        public string mysqlusr;
        public string mysqlpass;
        public string mysqlport;
        public string mysqldb;
        public string mysqltable;
        public string mysqltableitems;
        public List<Coordinate> mechanic;
        public class Coordinate
        {
            public float x;
            public float y;
            public float z;
        }
        public class blc
        {
            public ushort id;
        }


        public class CarBarricade
        {
            public ulong id;
            public List<Barr> BarricadeList;
        }

        public class Barr
        {
            public Barricade barricade;
            public ItemBarricadeAsset asset;
            public Transform hit;
            public ushort index;
            public byte x;
            public byte y;
            public ushort plant;
            public uint instanceID;
            public Vector3 point;
            public float angle_x;
            public float angle_y;
            public float angle_z;
            public ulong owner;
            public ulong group;
            public List<ItemJar> items;
            public bool SomeBool;
            public CSteamID Claimer;
            public ushort amount;
            public uint amountint;
            public ushort newShirt;
            public byte newShirtQuality;
            public byte[] newShirtState;
            public ushort newPants;
            public byte newPantsQuality;
            public byte[] newPantsState;
            public ushort newHat;
            public byte newHatQuality;
            public byte[] newHatState;
            public ushort newBackpack;
            public byte newBackpackQuality;
            public byte[] newBackpackState;
            public ushort newVest;
            public byte newVestQuality;
            public byte[] newVestState;
            public ushort newMask;
            public byte newMaskQuality;
            public byte[] newMaskState;
            public ushort newGlasses;
            public byte newGlassesQuality;
            public byte[] newGlassesState;
            public int newVisualShirt;
            public int newVisualPants;
            public int newVisualHat;
            public int newVisualBackpack;
            public int newVisualVest;
            public int newVisualMask;
            public int newVisualGlasses;
            public byte pose_comp;
            public ItemSentryAsset sentryAsset;
            public ESentryMode sentryMode;
            public string SomeText;
        }
    }
}
