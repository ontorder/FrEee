﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrEee.Game.Interfaces;

namespace FrEee.Game.Objects.Combat2
{
    public class CombatEmpire
    {
        public List<CombatObject> ownships = new List<CombatObject>();
        public List<CombatObject> friendly = new List<CombatObject>();
        public List<CombatObject> neutral = new List<CombatObject>(); //not currently used.
        public List<CombatObject> hostile = new List<CombatObject>();
        public CombatEmpire()
        { }
    }

    public class CombatObject
    {
        private ICombatant comObj;

        public CombatObject(ICombatant comObj)
        {
            this.comObj = comObj;
            Vehicles.SpaceVehicle ship = (Vehicles.SpaceVehicle)comObj;
            this.cmbt_mass = (double)ship.Size;
            this.maxfowardThrust = ship.Speed / this.cmbt_mass;
            this.maxStrafeThrust = (ship.Speed / this.cmbt_mass) / 4;
            this.Rotate = (ship.Speed / this.cmbt_mass) / 12;

            this.waypointTarget = new combatWaypoint();
            this.weaponTarget = new List<CombatObject>(1);//eventualy this should be something with the multiplex tracking component.
            
            this.cmbt_thrust = new Point3d(0, 0, 0);
            this.cmbt_accel = new Point3d(0, 0, 0);

        }

        /// <summary>
        /// location within the sector
        /// </summary>
        public Point3d cmbt_loc { get; set; }

        /// <summary>
        /// between phys tic locations. 
        /// </summary>
        public Point3d rndr_loc { get; set; }

        /// <summary>
        /// facing towards this point
        /// </summary>
        public Point3d cmbt_face { get; set; }

        /// <summary>
        /// combat velocity
        /// </summary>
        public Point3d cmbt_vel { get; set; }

        public Point3d cmbt_accel { get; set; }

        public Point3d cmbt_thrust { get; set; }

        public double cmbt_mass { get; set; }

        //public Point3d cmbt_maxThrust { get; set; }
        //public Point3d cmbt_minThrust { get; set; }

        public ICombatant icomobj
        {

            get { return this.comObj; }
            set { this.comObj = value; }
        }

        public combatWaypoint waypointTarget { get; set; }
        public Point3d lastVectortoWaypoint { get; set; }
        //public double lastDistancetoWaypoint { get; set; }

        public List<CombatObject> weaponTarget { get; set; }

        public double maxfowardThrust { get; set; }
        public double maxStrafeThrust { get; set; }
        public double Rotate { get; set; }

    }



    public class combatWaypoint
    {
        public combatWaypoint()
        {
            this.cmbt_loc = new Point3d(0, 0, 0);
            this.cmbt_vel = new Point3d(0, 0, 0);
        }
        public combatWaypoint(Point3d cmbt_loc)
        {
            this.cmbt_loc = cmbt_loc;
        }
        public combatWaypoint(CombatObject comObj)
        {
            this.comObj = comObj;
            this.cmbt_loc = comObj.cmbt_loc;
            this.cmbt_vel = comObj.cmbt_vel;
        }

        /// <summary>
        /// location within the sector
        /// </summary>
        public Point3d cmbt_loc { get; set; }

        /// <summary>
        /// combat velocity
        /// </summary>
        public Point3d cmbt_vel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CombatObject comObj { get; set; }

    }
}
